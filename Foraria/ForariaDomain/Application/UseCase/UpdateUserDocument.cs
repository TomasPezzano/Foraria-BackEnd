using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;
using System.Text.RegularExpressions;

namespace Foraria.Application.UseCase
{
    public class UpdateUserDocument
    {
        private readonly IUserDocumentRepository _userDocumentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserDocument(IUserDocumentRepository userDocumentRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userDocumentRepository = userDocumentRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDocument> ExecuteAsync(int documentId, int userId, string? title, string? description, string? category, string? url)
        {
            var document = await _userDocumentRepository.GetById(documentId)
                ?? throw new NotFoundException($"El documento con ID {documentId} no existe.");

            var user = await _userRepository.GetById(userId)
                ?? throw new NotFoundException($"El usuario con ID {userId} no existe.");

            bool isOwner = document.User_id == userId;
            bool isAdmin = user.Role.Description == "Administrador";
            bool isConsortium = user.Role.Description == "Consorcio";

            if (!isOwner && !isAdmin && !isConsortium)
                throw new UnauthorizedAccessException("No tiene permisos para modificar este documento.");

            if (!string.IsNullOrWhiteSpace(title))
                document.Title = title;

            if (!string.IsNullOrWhiteSpace(description))
                document.Description = description;

            if (!string.IsNullOrWhiteSpace(category))
            {
                var allowedCategories = new[] { "Contrato", "Reglamentos", "Actas", "Presupuestos", "Planos", "Seguros", "Manuales", "Emergencias", "Mantenimiento" }; //(?)
                if (!allowedCategories.Contains(document.Category, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException("La categoría del documento no es válida.");
                document.Category = category;
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                var urlRegex = new Regex(@"^https?:\/\/[\w\-\.]+(\.[\w\-]+)+[/#?]?.*$");
                if (!urlRegex.IsMatch(url))
                    throw new ArgumentException("La URL del documento no es válida.");

                var validExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".txt", ".xls", ".xlsx", ".csv", ".ppt", ".pptx", ".odt" };
                if (!validExtensions.Any(ext => document.Url.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("El formato del documento no es válido (solo .pdf, .jpg, .png, docx, txt, xls, xlsx, csv, ppt, pptx y odt).");

                document.Url = url;
            }

            _userDocumentRepository.Update(document);
            await _unitOfWork.SaveChangesAsync();

            return document;
        }
    }
}
