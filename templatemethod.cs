using System;

namespace TemplateMethodBeverages
{
    abstract class Beverage
    {
        public void PrepareRecipe()
        {
            BoilWater();
            Brew();           
            PourInCup();
            if (CustomerWantsCondiments()) 
            {
                AddCondiments();  
            }
            Console.WriteLine("Напиток готов!\n");
        }

        private void BoilWater()
        {
            Console.WriteLine("Кипятим воду...");
        }

        private void PourInCup()
        {
            Console.WriteLine("Наливаем напиток в чашку...");
        }

        protected abstract void Brew();
        protected abstract void AddCondiments();

        protected virtual bool CustomerWantsCondiments()
        {
            return true;
        }
    }

    class Tea : Beverage
    {
        protected override void Brew()
        {
            Console.WriteLine("Завариваем чайный пакетик...");
        }

        protected override void AddCondiments()
        {
            Console.WriteLine("Добавляем лимон...");
        }

        protected override bool CustomerWantsCondiments()
        {
            Console.Write("Хотите добавить лимон? (y/n): ");
            string input = Console.ReadLine()?.Trim().ToLower() ?? "n";
            while (input != "y" && input != "n")
            {
                Console.Write("Некорректный ввод. Введите y или n: ");
                input = Console.ReadLine()?.Trim().ToLower() ?? "n";
            }
            return input == "y";
        }
    }

    class Coffee : Beverage
    {
        protected override void Brew()
        {
            Console.WriteLine("Завариваем кофе через фильтр...");
        }

        protected override void AddCondiments()
        {
            Console.WriteLine("Добавляем сахар и молоко...");
        }

        protected override bool CustomerWantsCondiments()
        {
            Console.Write("Хотите добавить сахар и молоко? (y/n): ");
            string input = Console.ReadLine()?.Trim().ToLower() ?? "n";
            while (input != "y" && input != "n")
            {
                Console.Write("Некорректный ввод. Введите y или n: ");
                input = Console.ReadLine()?.Trim().ToLower() ?? "n";
            }
            return input == "y";
        }
    }

    class HotChocolate : Beverage
    {
        protected override void Brew()
        {
            Console.WriteLine("Смешиваем порошок какао с горячей водой...");
        }

        protected override void AddCondiments()
        {
            Console.WriteLine("Добавляем маршмеллоу...");
        }

        protected override bool CustomerWantsCondiments()
        {
            Console.Write("Хотите добавить маршмеллоу? (y/n): ");
            string input = Console.ReadLine()?.Trim().ToLower() ?? "n";
            while (input != "y" && input != "n")
            {
                Console.Write("Некорректный ввод. Введите y или n: ");
                input = Console.ReadLine()?.Trim().ToLower() ?? "n";
            }
            return input == "y";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Паттерн 'Шаблонный метод' ===");
            Console.WriteLine("Выберите напиток для приготовления:");
            Console.WriteLine("1 - Чай");
            Console.WriteLine("2 - Кофе");
            Console.WriteLine("3 - Горячий шоколад");
            Console.WriteLine("0 - Выход");

            while (true)
            {
                Console.Write("\nВведите номер напитка: ");
                string input = Console.ReadLine()?.Trim();
                if (input == "0") break;

                Beverage beverage = null;

                switch (input)
                {
                    case "1":
                        beverage = new Tea();
                        break;
                    case "2":
                        beverage = new Coffee();
                        break;
                    case "3":
                        beverage = new HotChocolate();
                        break;
                    default:
                        Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                        continue;
                }

                Console.WriteLine();
                beverage.PrepareRecipe();
            }

            Console.WriteLine("\nВыход из программы. До свидания!");
        }
    }
}
