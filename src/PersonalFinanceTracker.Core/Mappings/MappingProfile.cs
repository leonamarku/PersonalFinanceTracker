using AutoMapper;
using PersonalFinanceTracker.Core.DTOs.Account;
using PersonalFinanceTracker.Core.DTOs.Budget;
using PersonalFinanceTracker.Core.DTOs.Category;
using PersonalFinanceTracker.Core.DTOs.Transaction;
using PersonalFinanceTracker.Core.Entities;

namespace PersonalFinanceTracker.Core.Mappings;

/// <summary>AutoMapper profile — maps entities to DTOs and vice versa.</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Account
        CreateMap<Account, AccountDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()));
        CreateMap<CreateAccountDto, Account>();
        CreateMap<UpdateAccountDto, Account>();

        // Category
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()));
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        // Transaction
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.AccountName,    o => o.MapFrom(s => s.Account.Name))
            .ForMember(d => d.CategoryName,   o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryColor,  o => o.MapFrom(s => s.Category.Color))
            .ForMember(d => d.TypeName,       o => o.MapFrom(s => s.Type.ToString()));
        CreateMap<CreateTransactionDto, Transaction>();
        CreateMap<UpdateTransactionDto, Transaction>();

        // Budget
        CreateMap<Budget, BudgetDto>()
            .ForMember(d => d.CategoryName,  o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryColor, o => o.MapFrom(s => s.Category.Color));
        CreateMap<CreateBudgetDto, Budget>();
    }
}
