using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using System.Text.RegularExpressions;

namespace Foraria.Application.UseCase
{
    public interface ICreateUserDocument
    {
        Task<UserDocument> Execute(UserDocument document);
    }

    public class CreateUserDocument : ICreateUserDocument
    {
        private readonly IUserDocumentRepository _userDocumentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConsortiumRepository _consortiumRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserDocument(
            IUserDocumentRepository userDocumentRepository,
            IUserRepository userRepository,
            IConsortiumRepository consortiumRepository,
            IUnitOfWork unitOfWork)
        {
            _userDocumentRepository = userDocumentRepository;
            _userRepository = userRepository;
            _consortiumRepository = consortiumRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDocument> Execute(UserDocument document)
        {
            var user = await _userRepository.GetById(document.User_id)
                ?? throw new NotFoundException($"El usuario con ID {document.User_id} no existe.");

            var consortium = await _consortiumRepository.FindById(document.Consortium_id)
                ?? throw new NotFoundException($"El consorcio con ID {document.Consortium_id} no existe.");

            ValidateDocument(document);

            document.CreatedAt = DateTime.UtcNow;
            document.IsValid = true;

            await _userDocumentRepository.Add(document);
            await _unitOfWork.SaveChangesAsync();

            return document;
        }

        private void ValidateDocument(UserDocument document)
        {
            var urlRegex = new Regex(@"^https?:\/\/[\w\-\.]+(\.[\w\-]+)+[/#?]?.*$");
            if (!urlRegex.IsMatch(document.Url))
                throw new ArgumentException("La URL del documento no es válida.");

            var allowedCategories = new[] { "Contrato", "Reglamentos", "Actas", "Presupuestos", "Planos", "Seguros", "Manuales", "Emergencias", "Mantenimiento"}; //(?)
            if (!allowedCategories.Contains(document.Category, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("La categoría del documento no es válida.");

            var validExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".txt", ".xls", ".xlsx", ".csv", ".ppt", ".pptx", ".odt" };
            if (!validExtensions.Any(ext => document.Url.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("El formato del documento no es válido (solo .pdf, .jpg, .png, docx, txt, xls, xlsx, csv, ppt, pptx y odt).");
        }
    }
}
