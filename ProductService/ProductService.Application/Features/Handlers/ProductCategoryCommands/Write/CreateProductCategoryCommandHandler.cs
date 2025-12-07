using MediatR;
using ProductService.Application.Features.Commands.ProductCategoryCommands;
using ProductService.Application.Repositories;
using ProductService.Application.UnitOfWorks;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Handlers.ProductCategoryCommands.Write;

public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand>
{
    private readonly IGenericRepository<ProductCategory>  _productCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    public CreateProductCategoryCommandHandler(IGenericRepository<ProductCategory> productCategoryRepository, IUnitOfWork unitOfWork)
    {
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var productCategory = new ProductCategory();
            productCategory.Id = Guid.NewGuid();
            productCategory.Name = request.Name;
            productCategory.Description = request.Description;
            
            await _productCategoryRepository.CreateAsync(productCategory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

}