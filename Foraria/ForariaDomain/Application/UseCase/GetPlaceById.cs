using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetPlaceById
{
    Task<Place?> Execute(int id);
}
public class GetPlaceById : IGetPlaceById
{
    private readonly IPlaceRepository _placeRepository;
    public GetPlaceById(IPlaceRepository placeRepository)
    {
        _placeRepository = placeRepository;
    }
    public async Task<Place?> Execute(int id)
    {
        return await _placeRepository.GetById(id);
    }
}
