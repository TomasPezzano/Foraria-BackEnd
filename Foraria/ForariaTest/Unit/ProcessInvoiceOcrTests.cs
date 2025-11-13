using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Services;
using System.Text;

namespace ForariaTest.Unit;

public class ProcessInvoiceOcrTests
{
    private IFormFile CreateFakeFile(string fileName = "factura.pdf", string content = "texto")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResult_WhenOcrSuccess()
    {
        // Arrange
        var file = CreateFakeFile();

        var ocrResult = new InvoiceOcrResult
        {
            Success = true,
            SupplierName = "Proveedor",
            Cuit = "20-12345678-9",
            InvoiceNumber = "A-001",
            TotalAmount = 1234.56m
        };

        var mockOcr = new Mock<IOcrService>();
        mockOcr.Setup(o => o.ProcessInvoiceAsync(file)).ReturnsAsync(ocrResult);

        var mockLogger = new Mock<ILogger<ProcessInvoiceOcr>>();

        var mockStorage = new Mock<ILocalFileStorageService>();
        mockStorage.Setup(s => s.SaveInvoiceFileAsync(It.IsAny<Stream>(), file.FileName))
                   .ReturnsAsync("ruta/guardada/factura.pdf");

        var useCase = new ProcessInvoiceOcr(mockOcr.Object, mockLogger.Object, mockStorage.Object);

        // Act
        var result = await useCase.ExecuteAsync(file);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Proveedor", result.SupplierName);
        Assert.Equal("20-12345678-9", result.Cuit);
        Assert.Equal("A-001", result.InvoiceNumber);
        Assert.Equal("ruta/guardada/factura.pdf", result.FilePath);

        mockStorage.Verify(s => s.SaveInvoiceFileAsync(It.IsAny<Stream>(), file.FileName), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResult_WhenOcrFails()
    {
        // Arrange
        var file = CreateFakeFile();

        var ocrResult = new InvoiceOcrResult
        {
            Success = false,
            ErrorMessage = "OCR falló"
        };

        var mockOcr = new Mock<IOcrService>();
        mockOcr.Setup(o => o.ProcessInvoiceAsync(file)).ReturnsAsync(ocrResult);

        var mockLogger = new Mock<ILogger<ProcessInvoiceOcr>>();
        var mockStorage = new Mock<ILocalFileStorageService>();

        var useCase = new ProcessInvoiceOcr(mockOcr.Object, mockLogger.Object, mockStorage.Object);

        // Act
        var result = await useCase.ExecuteAsync(file);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("OCR falló", result.ErrorMessage);

        mockStorage.Verify(s => s.SaveInvoiceFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnErrorResult_WhenExceptionThrown()
    {
        // Arrange
        var file = CreateFakeFile();

        var mockOcr = new Mock<IOcrService>();
        mockOcr.Setup(o => o.ProcessInvoiceAsync(file))
               .ThrowsAsync(new Exception("Error inesperado"));

        var mockLogger = new Mock<ILogger<ProcessInvoiceOcr>>();
        var mockStorage = new Mock<ILocalFileStorageService>();

        var useCase = new ProcessInvoiceOcr(mockOcr.Object, mockLogger.Object, mockStorage.Object);

        // Act
        var result = await useCase.ExecuteAsync(file);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error interno al procesar", result.ErrorMessage);
        Assert.Contains("Error inesperado", result.ErrorMessage);

        mockStorage.Verify(s => s.SaveInvoiceFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_AndWarningOnFail()
    {
        // Arrange
        var file = CreateFakeFile();

        var ocrResult = new InvoiceOcrResult
        {
            Success = false,
            ErrorMessage = "OCR inválido"
        };

        var mockOcr = new Mock<IOcrService>();
        mockOcr.Setup(x => x.ProcessInvoiceAsync(file)).ReturnsAsync(ocrResult);

        var mockLogger = new Mock<ILogger<ProcessInvoiceOcr>>();
        var mockStorage = new Mock<ILocalFileStorageService>();

        var useCase = new ProcessInvoiceOcr(mockOcr.Object, mockLogger.Object, mockStorage.Object);

        // Act
        var result = await useCase.ExecuteAsync(file);

        // Assert básico de funcionamiento
        Assert.False(result.Success);

        // Verificación de que se llamó a LogWarning
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
