using Application.Services;
using Domain.Abstractions;
using Infrastructure;

IAccountRepository accounts = new InMemoryAccountRepository();
IInterestRuleRepository rules = new InMemoryInterestRuleRepository();

var txnSvc  = new TransactionService(accounts);
var ruleSvc = new InterestRuleService(rules);
var stmtSvc = new StatementService(accounts, rules);

while (true)
{
    Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
    Console.WriteLine("[T] Input transactions ");
    Console.WriteLine("[I] Define interest rules");
    Console.WriteLine("[P] Print statement");
    Console.WriteLine("[Q] Quit");
    Console.Write("> ");

    var choice = Console.ReadLine()?.Trim().ToUpperInvariant();
    if (string.IsNullOrEmpty(choice)) continue;

    try
    {
        switch (choice)
        {
            case "T":
                Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format ");
                Console.WriteLine("(or enter blank to go back to main menu):");
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4) { Console.WriteLine("Invalid format"); break; }

                var (okT, errT, acc) = txnSvc.TryAddUserTransaction(parts[0], parts[1], parts[2], parts[3]);
                if (!okT) { Console.WriteLine(errT); break; }
                var (okS, _, text) = stmtSvc.TryPrintAccountAll(parts[1]);
                Console.WriteLine(text);
                break;

            case "I":
                Console.WriteLine("Please enter interest rules details in <Date> <RuleId> <Rate in %> format ");
                Console.WriteLine("(or enter blank to go back to main menu):");
                Console.Write("> ");
                var l2 = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(l2)) break;
                var p2 = l2.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (p2.Length != 3) { Console.WriteLine("Invalid format"); break; }

                var (okR, errR) = ruleSvc.TryUpsert(p2[0], p2[1], p2[2]);
                if (!okR) { Console.WriteLine(errR); break; }

                Console.WriteLine("Interest rules:");
                Console.WriteLine("| Date     | RuleId | Rate (%) |");
                foreach (var r in ruleSvc.ListOrdered())
                    Console.WriteLine($"| {r.EffectiveDate:yyyyMMdd} | {r.RuleId,-6} | {r.RatePercent,8:0.00} |");
                break;

            case "P":
                Console.WriteLine("Please enter account and month to generate the statement <Account> <Year><Month>");
                Console.WriteLine("(or enter blank to go back to main menu):");
                Console.Write("> ");
                var l3 = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(l3)) break;
                var p3 = l3.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (p3.Length != 2) { Console.WriteLine("Invalid format"); break; }
                var (okM, errM, outM) = stmtSvc.TryPrintMonthly(p3[0], p3[1]);
                Console.WriteLine(okM ? outM : errM);
                break;

            case "Q":
                Console.WriteLine("Thank you for banking with AwesomeGIC Bank.");
                Console.WriteLine("Have a nice day!");
                return;

            default:
                Console.WriteLine("Unknown option.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine("\nIs there anything else you'd like to do?");
}
