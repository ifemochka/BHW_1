using System;
using System.Diagnostics;

public class BankAccount
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Balance { get; set; }

    public BankAccount(int id, string name, int balance)
    {
        Id = id;
        Name = name;
        Balance = balance;
    }

    public BankAccount(int id, string name)
    {
        Id = id;
        Name = name;
        Balance = 0;
    }

    public void UpdateBalance(int amount)
    {
        Balance += amount;
    }

    public void SetBalance(int balance)
    {
        Balance = balance;
    }
}


public class Category
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }

    public Category(int id, string type, string name)
    {
        Id = id;
        Type = type;
        Name = name;
    }
}


public class Operation
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int BankAccountId { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }

    public Operation(int id, string type, int bankAccountId, int amount, DateTime date, string description, int categoryId)
    {
        Id = id;
        Type = type;
        BankAccountId = bankAccountId;
        Amount = amount;
        Date = date;
        Description = description;
        CategoryId = categoryId;
    }
}


public class BankAccountFacade
{
    private List<BankAccount> accounts = new List<BankAccount>();

    public void CreateAccount(int id, string name)
    {
        var account = new BankAccount(id, name);
        accounts.Add(account);
    }

    public void UpdateAccount(int accountId, string name)
    {
        var account = accounts.Find(a => a.Id == accountId);
        if (account != null)
        {
            accounts.Remove(account);
            accounts.Add(new BankAccount(accountId, name) { Balance = account.Balance });
        }
    }

    public void DeleteAccount(int accountId)
    {
        accounts.RemoveAll(a => a.Id == accountId);
    }

    public BankAccount GetAccount(int accountId)
    {
        return accounts.Find(a => a.Id == accountId);
    }

    public List<BankAccount> GetAllAccounts() => accounts;
}


public class CategoryFacade
{
    private List<Category> categories = new List<Category>();

    public void CreateCategory(int id, string type, string name)
    {
        var category = new Category(id, type, name);
        categories.Add(category);
    }

    public void UpdateCategory(int categoryId, string name)
    {
        var category = categories.Find(c => c.Id == categoryId);
        if (category != null)
        {
            categories.Remove(category);
            categories.Add(new Category(categoryId, category.Type, name));
        }
    }

    public void DeleteCategory(int categoryId)
    {
        categories.RemoveAll(c => c.Id == categoryId);
    }

    public List<Category> GetAllCategories() => categories;
}


public class OperationFacade
{
    private List<Operation> operations = new List<Operation>();

    public void CreateOperation(int id, string type, int bankAccountId, int amount, DateTime date, string description, int categoryId)
    {
        var operation = new Operation(id, type, bankAccountId, amount, date, description, categoryId);
        operations.Add(operation);

        var accountFacade = new BankAccountFacade();
        var account = accountFacade.GetAccount(bankAccountId);
        if (account != null)
        {
            account.UpdateBalance(type == "income" ? amount : -amount);
        }
    }

    public void DeleteOperation(int operationId)
    {
        var operation = operations.Find(o => o.Id == operationId);
        if (operation != null)
        {
            operations.Remove(operation);
            var accountFacade = new BankAccountFacade();
            var account = accountFacade.GetAccount(operation.BankAccountId);
            if (account != null)
            {
                account.UpdateBalance(operation.Type == "income" ? -operation.Amount : operation.Amount);
            }
        }
    }

    public List<Operation> GetAllOperations() => operations;

    public void RecalculateBalance(BankAccount account)
    {
        var balanceFromOperations = operations
            .Where(op => op.BankAccountId == account.Id)
            .Sum(op => op.Type == "income" ? op.Amount : -op.Amount);

        account.SetBalance(balanceFromOperations);
    }
}


public class AnalyticsFacade
{
    private readonly List<Operation> _operations;

    public AnalyticsFacade(List<Operation> operations)
    {
        _operations = operations;
    }

    public int GetIncomeExpenseDifference(DateTime startDate, DateTime endDate)
    {
        var income = _operations.Where(o => o.Type == "income" && o.Date >= startDate && o.Date <= endDate).Sum(o => o.Amount);
        var expense = _operations.Where(o => o.Type == "expense" && o.Date >= startDate && o.Date <= endDate).Sum(o => o.Amount);
        return income - expense;
    }

    public Dictionary<string, int> GroupByCategory(DateTime startDate, DateTime endDate)
    {
        return _operations
            .Where(o => o.Date >= startDate && o.Date <= endDate)
            .GroupBy(o => o.CategoryId)
            .ToDictionary(g => g.Key.ToString(), g => g.Sum(o => o.Type == "income" ? o.Amount : -o.Amount));
    }
}


public interface ICommand
{
    void Execute();
}

public class AddOperationCommand : ICommand
{
    private Operation operation;

    public AddOperationCommand(Operation operation)
    {
        this.operation = operation;
    }

    public void Execute() { }
}

public class CommandDecorator : ICommand
{
    private ICommand command;

    public CommandDecorator(ICommand command)
    {
        this.command = command;
    }

    public void Execute()
    {
        var stopwatch = Stopwatch.StartNew();
        command.Execute();
        stopwatch.Stop();

        Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
    }
}


public abstract class DataImporter
{
    public void ImportData(string filePath)
    {
        var data = ReadFile(filePath);
        ParseData(data);
        SaveData(data);
    }

    protected abstract string ReadFile(string filePath);

    protected abstract void ParseData(string data);

    protected abstract void SaveData(string parsedData);
}

public class JsonDataImporter : DataImporter
{
    protected override string ReadFile(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    protected override void ParseData(string data) { }

    protected override void SaveData(string parsedData) { }
}


public interface IVisitor
{
    void Visit(BankAccount account);
    void Visit(Category category);
}

public class DataExportVisitor : IVisitor
{
    public void Visit(BankAccount account) { }
    public void Visit(Category category) { }
}


public static class FinancialObjectFactory
{
    public static Operation CreateOperation(int id, string type, int bankAccountId, int amount, DateTime date, string description, int categoryId)
    {
        return new Operation(id, type, bankAccountId, amount, date, description, categoryId);
    }
}


public class InMemoryCacheProxy
{
    private Dictionary<int, BankAccount> cache = new Dictionary<int, BankAccount>();

    public BankAccount GetBankAccount(int id)
    {
        if (!cache.ContainsKey(id))
        {
            var account = LoadFromDatabase(id);
            cache[id] = account;
        }

        return cache[id];
    }

    public void SaveBankAccount(BankAccount account)
    {
        cache[account.Id] = account;
        SaveToDatabase(account);
    }

    private BankAccount LoadFromDatabase(int id)
    {
        return new BankAccount(id, "bank");
    }

    private void SaveToDatabase(BankAccount account) { }
}


class Program
{
    static void Main()
    {
        int bankId = 0;
        int categotyId = 0;
        int operationId = 0;
        var bankAccountFacade = new BankAccountFacade();
        var categoryFacade = new CategoryFacade();
        var operationFacade = new OperationFacade();
        var analyticsFacade = new AnalyticsFacade(operationFacade.GetAllOperations());

        while (true)
        {
            Console.WriteLine("1. Добавить счет");
            Console.WriteLine("2. Добавить категорию");
            Console.WriteLine("3. Добавить транзакцию");
            Console.WriteLine("0. Выход");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Введите название счета: ");
                    var accountName = Console.ReadLine();

                    bankAccountFacade.CreateAccount(bankId++, accountName);
                    Console.WriteLine($"Счёт {accountName} создан. Id: {bankId}\n");
                    break;

                case "2":
                    Console.Write("Введите тип категории: ");
                    var categoryType = Console.ReadLine();
                    Console.Write("Введите название категории: ");
                    var categoryName = Console.ReadLine();

                    categoryFacade.CreateCategory(categotyId++, categoryType, categoryName);
                    Console.WriteLine($"Категория {categoryName} создана. Id {categotyId}\n");
                    break;

                case "3":
                    Console.Write("Введите ID счета: ");
                    var accountId = int.Parse(Console.ReadLine());
                    Console.Write("Введите тип операции (дебет/кредит): ");
                    var operationType = Console.ReadLine();
                    Console.Write("Введите сумму операции: ");
                    var amount = int.Parse(Console.ReadLine());
                    Console.Write("Введите описание операции: ");
                    var description = Console.ReadLine();
                    DateTime date;
                    string[] formats = { "dd.MM.yyyy" };
                    Console.Write("Введите дату в формате dd.MM.yyyy: ");
                    string input = Console.ReadLine();
                    DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                    Console.Write("Введите ID категории: ");
                    var categoryId = int.Parse(Console.ReadLine());

                    operationFacade.CreateOperation(operationId++,operationType, accountId, amount, date, description, categoryId);
                    Console.WriteLine($"Транзакция произведена. Id: {operationId}\n");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте еще раз.");
                    break;
            }
        }
    }
}
