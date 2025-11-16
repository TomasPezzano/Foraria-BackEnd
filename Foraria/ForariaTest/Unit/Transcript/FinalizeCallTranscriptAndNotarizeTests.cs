using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using Foraria.Domain.Model;
using Foraria.Domain.Service;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;

public class FinalizeCallTranscriptAndNotarizeTests
{
    private (
        Mock<ICallTranscriptRepository> transcriptRepo,
        Mock<IBlockchainService> blockchain,
        Mock<IBlockchainProofRepository> proofRepo,
        Mock<IUnitOfWork> uow,
        FinalizeCallTranscriptionAndNotarize useCase
    ) Create()
    {
        var transcriptRepo = new Mock<ICallTranscriptRepository>();
        var blockchain = new Mock<IBlockchainService>();
        var proofRepo = new Mock<IBlockchainProofRepository>();
        var uow = new Mock<IUnitOfWork>();

        var useCase = new FinalizeCallTranscriptionAndNotarize(
            transcriptRepo.Object,
            blockchain.Object,
            proofRepo.Object,
            uow.Object
        );

        return (transcriptRepo, blockchain, proofRepo, uow, useCase);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrow_NotFound_WhenTranscriptDoesNotExist()
    {
        // Arrange
        var (transcriptRepo, blockchain, proofRepo, uow, useCase) = Create();

        int transcriptId = 10;

        transcriptRepo.Setup(r => r.GetById(transcriptId))
                      .Returns((CallTranscript?)null);

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(transcriptId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Transcripción no encontrada.");
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenTranscriptHasNoHash()
    {
        // Arrange
        var (transcriptRepo, blockchain, proofRepo, uow, useCase) = Create();

        int transcriptId = 10;

        var transcript = new CallTranscript
        {
            Id = transcriptId,
            CallId = 2,
            TranscriptHash = ""
        };

        transcriptRepo.Setup(r => r.GetById(transcriptId))
                      .Returns(transcript);

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(transcriptId);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("La transcripción no tiene hash asociado.");
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenProofAlreadyExistsForTranscript()
    {
        // Arrange
        var (transcriptRepo, blockchain, proofRepo, uow, useCase) = Create();

        int transcriptId = 10;

        var transcript = new CallTranscript
        {
            Id = transcriptId,
            CallId = 1,
            TranscriptHash = "abc123"
        };

        transcriptRepo.Setup(r => r.GetById(transcriptId))
                      .Returns(transcript);

        proofRepo.Setup(r => r.GetByCallTranscriptIdAsync(transcriptId))
                 .ReturnsAsync(new BlockchainProof());

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(transcriptId);

        // Assert
        await act.Should().ThrowAsync<BlockchainException>()
            .WithMessage("Esta transcripción ya está notarizada");
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenHashAlreadyNotarized()
    {
        // Arrange
        var (transcriptRepo, blockchain, proofRepo, uow, useCase) = Create();

        int transcriptId = 10;

        var transcript = new CallTranscript
        {
            Id = transcriptId,
            CallId = 20,
            TranscriptHash = "h123"
        };

        transcriptRepo.Setup(r => r.GetById(transcriptId))
                      .Returns(transcript);

        proofRepo.Setup(r => r.GetByCallTranscriptIdAsync(transcriptId))
                 .ReturnsAsync((BlockchainProof?)null);

        proofRepo.Setup(r => r.GetByHashHexAsync("h123"))
                 .ReturnsAsync(new BlockchainProof());

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(transcriptId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("El archivo ya fue notarizado previamente*");
    }


    [Fact]
    public async Task ExecuteAsync_ShouldNotarizeAndSaveProof_WhenValid()
    {
        // Arrange
        var (transcriptRepo, blockchain, proofRepo, uow, useCase) = Create();

        int transcriptId = 10;

        var transcript = new CallTranscript
        {
            Id = transcriptId,
            CallId = 99,
            TranscriptHash = "hashABC"
        };

        transcriptRepo.Setup(r => r.GetById(transcriptId))
                      .Returns(transcript);

        proofRepo.Setup(r => r.GetByCallTranscriptIdAsync(transcriptId))
                 .ReturnsAsync((BlockchainProof?)null);

        proofRepo.Setup(r => r.GetByHashHexAsync("hashABC"))
                 .ReturnsAsync((BlockchainProof?)null);

        blockchain.Setup(b => b.NotarizeAsync("hashABC", "call-transcript:99"))
                  .ReturnsAsync(("tx789", "FINALHEX"));

        blockchain.Setup(b => b.ContractAddress)
                  .Returns("0xContract");

        // Act
        await useCase.ExecuteAsync(transcriptId);

        // Assert
        transcript.BlockchainTxHash.Should().Be("tx789");

        proofRepo.Verify(r => r.Add(It.Is<BlockchainProof>(p =>
            p.CallTranscriptId == transcriptId &&
            p.HashHex == "FINALHEX" &&
            p.Uri == "call-transcript:99" &&
            p.TxHash == "tx789" &&
            p.Contract == "0xContract" &&
            p.Network == "polygon" &&
            p.ChainId == 80002
        )), Times.Once);

        transcriptRepo.Verify(r => r.Update(transcript), Times.Once);

        uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
