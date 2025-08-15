Here is a complete and well-formatted `README.md` file for your project `AwesomeGicBank`, suitable for uploading to GitHub:

---

````markdown
# AwesomeGicBank

A modular, testable, and production-ready .NET 8 banking system designed for clean architecture, SOLID principles, and extensibility.

---

## Design Overview

### Architecture
This project follows **Clean Architecture**:

- **Domain**: Core entities and enums (`Account`, `Transaction`, `InterestRule`)
- **Application**: Business services (`TransactionService`, `InterestRuleService`)
- **Infrastructure**: In-memory data storage (simulating persistence)
- **Cli**: Console-based interface to simulate application behavior
- **Tests**: Unit tests using xUnit

### Key Features
- Deposit, Withdrawal, and Interest Transactions
- No overdraft allowed (validated in chronological order)
- First transaction must be a deposit
- Transaction validation (date, amount, type)
- Interest rule management
- Error handling with structured responses or exceptions
- xUnit-based tests with proper edge case coverage

### Assumptions
- All transaction amounts are positive and up to two decimal places.
- Only in-memory persistence is implemented.
- No REST API included in this version (can be added via ASP.NET Core).

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Git (for cloning)
- Supported OS: Windows, macOS, Linux

---

## Setup & Run

### 1. Clone the repo

```bash
git clone https://github.com/MGlolzz/AwesomeGicBank.git
cd AwesomeGicBank
````

### 2. Restore and build the solution

```bash
dotnet restore
dotnet build AwesomeGicBank.sln
```

### 3. Run the CLI app

```bash
cd src/Cli
dotnet run
```

### 4. Run Unit Tests

```bash
cd tests/Tests
dotnet test
```

> The test suite uses **xUnit** and covers the major services including validation, edge cases, and expected failure conditions.

