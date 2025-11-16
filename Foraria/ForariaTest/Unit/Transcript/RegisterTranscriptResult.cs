using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Repository;

public class RegisterTranscriptionResultTests
{
    [Fact]
    public void Execute_ShouldUpdateExistingTranscript_WhenTranscriptExists()
    {
        // Arrange
        int callId = 10;

        var existingTranscript = new CallTranscript
        {
            Id = 1,
            CallId = callId,
            TranscriptPath = "old.txt",
            AudioPath = "old.mp3",
            TranscriptHash = "oldHash",
            AudioHash = "oldAudioHash"
        };

        var mockRepo = new Mock<ICallTranscriptRepository>();
        mockRepo.Setup(r => r.GetByCallId(callId)).Returns(existingTranscript);

        var useCase = new RegisterTranscriptionResult(mockRepo.Object);

        // Act
        var result = useCase.Execute(
            callId,
            transcriptPath: "new.txt",
            audioPath: "new.mp3",
            transcriptHash: "newHash",
            audioHash: "newAudioHash"
        );

        // Assert
        result.Should().Be(existingTranscript);

        result.TranscriptPath.Should().Be("new.txt");
        result.AudioPath.Should().Be("new.mp3");
        result.TranscriptHash.Should().Be("newHash");
        result.AudioHash.Should().Be("newAudioHash");

        mockRepo.Verify(r => r.Update(existingTranscript), Times.Once);
        mockRepo.Verify(r => r.Create(It.IsAny<CallTranscript>()), Times.Never);
    }

    [Fact]
    public void Execute_ShouldCreateNewTranscript_WhenNoneExists()
    {
        // Arrange
        int callId = 10;

        var newTranscript = new CallTranscript
        {
            Id = 99,
            CallId = callId,
            TranscriptPath = "new.txt",
            AudioPath = "new.mp3",
            TranscriptHash = "newHash",
            AudioHash = "newAudioHash"
        };

        var mockRepo = new Mock<ICallTranscriptRepository>();
        mockRepo.Setup(r => r.GetByCallId(callId)).Returns((CallTranscript?)null);

        mockRepo.Setup(r => r.Create(It.IsAny<CallTranscript>()))
                .Returns(newTranscript);

        var useCase = new RegisterTranscriptionResult(mockRepo.Object);

        // Act
        var result = useCase.Execute(
            callId,
            "new.txt",
            "new.mp3",
            "newHash",
            "newAudioHash"
        );

        // Assert
        result.Should().Be(newTranscript);

        mockRepo.Verify(r => r.Create(It.Is<CallTranscript>(t =>
            t.CallId == callId &&
            t.TranscriptPath == "new.txt" &&
            t.AudioPath == "new.mp3" &&
            t.TranscriptHash == "newHash" &&
            t.AudioHash == "newAudioHash"
        )), Times.Once);

        mockRepo.Verify(r => r.Update(It.IsAny<CallTranscript>()), Times.Never);
    }
}
