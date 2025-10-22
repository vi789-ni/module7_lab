using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandPatternSmartHome
{
    public interface ICommand
    {
        void Execute();
        void Undo();
        string Name { get; } 
    }

    public class Light
    {
        public string Location { get; }
        public bool IsOn { get; private set; }
        public Light(string location) => Location = location;
        public void On()
        {
            IsOn = true;
            Console.WriteLine($"[Light] {Location}: ON");
        }
        public void Off()
        {
            IsOn = false;
            Console.WriteLine($"[Light] {Location}: OFF");
        }
    }

    public class Door
    {
        public string Name { get; }
        public bool IsOpen { get; private set; }
        public Door(string name) => Name = name;
        public void Open()
        {
            IsOpen = true;
            Console.WriteLine($"[Door] {Name}: OPEN");
        }
        public void Close()
        {
            IsOpen = false;
            Console.WriteLine($"[Door] {Name}: CLOSED");
        }
    }

    public class Thermostat
    {
        public int Temperature { get; private set; }
        public Thermostat(int initial = 22) => Temperature = initial;
        public void SetTemperature(int t)
        {
            Temperature = t;
            Console.WriteLine($"[Thermostat] Temperature set to {Temperature}°C");
        }
    }

    public class TV
    {
        public string Name { get; }
        public bool IsOn { get; private set; }
        public TV(string name) => Name = name;
        public void Toggle()
        {
            IsOn = !IsOn;
            Console.WriteLine($"[TV] {Name}: {(IsOn ? "ON" : "OFF")}");
        }
    }

    public class LightOnCommand : ICommand
    {
        private readonly Light _light;
        public LightOnCommand(Light light) => _light = light;
        public string Name => $"LightOn({_light.Location})";
        public void Execute() => _light.On();
        public void Undo() => _light.Off();
    }

    public class LightOffCommand : ICommand
    {
        private readonly Light _light;
        public LightOffCommand(Light light) => _light = light;
        public string Name => $"LightOff({_light.Location})";
        public void Execute() => _light.Off();
        public void Undo() => _light.On();
    }

    public class DoorOpenCommand : ICommand
    {
        private readonly Door _door;
        public DoorOpenCommand(Door door) => _door = door;
        public string Name => $"DoorOpen({_door.Name})";
        public void Execute() => _door.Open();
        public void Undo() => _door.Close();
    }

    public class DoorCloseCommand : ICommand
    {
        private readonly Door _door;
        public DoorCloseCommand(Door door) => _door = door;
        public string Name => $"DoorClose({_door.Name})";
        public void Execute() => _door.Close();
        public void Undo() => _door.Open();
    }

    public class ThermostatSetCommand : ICommand
    {
        private readonly Thermostat _thermostat;
        private readonly int _newTemp;
        private int _prevTemp;
        public ThermostatSetCommand(Thermostat thermostat, int newTemp)
        {
            _thermostat = thermostat;
            _newTemp = newTemp;
        }
        public string Name => $"ThermostatSet({_newTemp}°C)";
        public void Execute()
        {
            _prevTemp = _thermostat.Temperature;
            _thermostat.SetTemperature(_newTemp);
        }
        public void Undo() => _thermostat.SetTemperature(_prevTemp);
    }

    public class TVToggleCommand : ICommand
    {
        private readonly TV _tv;
        public TVToggleCommand(TV tv) => _tv = tv;
        public string Name => $"TVToggle({_tv.Name})";
        public void Execute() => _tv.Toggle();
        public void Undo() => _tv.Toggle(); 
    }

    public class RemoteInvoker
    {
        private readonly Stack<ICommand> _history;
        private readonly int _maxHistorySize;

        public RemoteInvoker(int maxHistorySize = 100)
        {
            _history = new Stack<ICommand>();
            _maxHistorySize = Math.Max(1, maxHistorySize);
        }

        public void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            command.Execute();

            _history.Push(command);
            if (_history.Count > _maxHistorySize)
            {
                var arr = _history.ToArray(); 
                Array.Reverse(arr); 
                var trimmed = arr.Skip(arr.Length - _maxHistorySize).Reverse().ToArray();
                _history.Clear();
                foreach (var c in trimmed) _history.Push(c);
            }
        }

        public void UndoLast()
        {
            if (!_history.Any())
            {
                Console.WriteLine("Нет команд для отмены.");
                return;
            }
            var cmd = _history.Pop();
            try
            {
                cmd.Undo();
                Console.WriteLine($"Отменена команда: {cmd.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отмене команды {cmd.Name}: {ex.Message}");
            }
        }

        public void UndoMultiple(int n)
        {
            if (n <= 0)
            {
                Console.WriteLine("Введите положительное число команд для отмены.");
                return;
            }
            for (int i = 0; i < n; i++)
            {
                if (!_history.Any())
                {
                    Console.WriteLine("Больше команд для отмены нет.");
                    break;
                }
                UndoLast();
            }
        }

        public void PrintHistory()
        {
            if (!_history.Any())
            {
                Console.WriteLine("История пуста.");
                return;
            }
            Console.WriteLine("История (последние сверху):");
            foreach (var cmd in _history.ToArray()) Console.WriteLine($" - {cmd.Name}");
        }

        public int HistoryCount => _history.Count;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" Smart Home ");
            var livingLight = new Light("Living Room");
            var kitchenLight = new Light("Kitchen");
            var frontDoor = new Door("Front Door");
            var backDoor = new Door("Back Door");
            var thermostat = new Thermostat(22);
            var tv = new TV("LivingRoomTV");

            var invoker = new RemoteInvoker(maxHistorySize: 50);

            while (true)
            {
                PrintMenu();
                var choice = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(choice)) { Console.WriteLine("Введите команду."); continue; }
                if (choice == "0") break;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            invoker.ExecuteCommand(new LightOnCommand(livingLight));
                            break;
                        case "2":
                            invoker.ExecuteCommand(new LightOffCommand(kitchenLight));
                            break;
                        case "3":
                            invoker.ExecuteCommand(new DoorOpenCommand(frontDoor));
                            break;
                        case "4":
                            invoker.ExecuteCommand(new DoorCloseCommand(frontDoor));
                            break;
                        case "5":
                            Console.Write("Введите температуру (целое число °C): ");
                            if (int.TryParse(Console.ReadLine(), out int t))
                                invoker.ExecuteCommand(new ThermostatSetCommand(thermostat, t));
                            else Console.WriteLine("Некорректный ввод температуры.");
                            break;
                        case "6":
                            invoker.ExecuteCommand(new TVToggleCommand(tv));
                            break;
                        case "7":
                            invoker.UndoLast();
                            break;
                        case "8":
                            Console.Write("Сколько команд отменить? ");
                            if (int.TryParse(Console.ReadLine(), out int n))
                                invoker.UndoMultiple(n);
                            else Console.WriteLine("Некорректный ввод числа.");
                            break;
                        case "9":
                            invoker.PrintHistory();
                            break;
                        case "10":
                            var macro = new List<ICommand>
                            {
                                new LightOnCommand(kitchenLight),
                                new LightOnCommand(livingLight),
                                new TVToggleCommand(tv)
                            };
                            Console.WriteLine("Выполняется макрокоманда (kitchen on, living on, tv toggle)...");
                            foreach (var c in macro) invoker.ExecuteCommand(c);
                            break;
                        default:
                            Console.WriteLine("Неизвестная команда. Попробуйте снова.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
            }

            Console.WriteLine("Выход. До встречи!");
        }

        static void PrintMenu()
        {
            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1  - Включить свет (Living Room)");
            Console.WriteLine("2  - Выключить свет (Kitchen)");
            Console.WriteLine("3  - Открыть дверь (Front Door)");
            Console.WriteLine("4  - Закрыть дверь (Front Door)");
            Console.WriteLine("5  - Установить температуру (Thermostat)");
            Console.WriteLine("6  - Переключить TV (Toggle)");
            Console.WriteLine("7  - Отменить последнюю команду (UndoLast)");
            Console.WriteLine("8  - Отменить несколько команд (UndoMultiple)");
            Console.WriteLine("9  - Показать историю команд");
            Console.WriteLine("10 - Выполнить макрокоманду (пример)");
            Console.WriteLine("0  - Выход");
            Console.Write(">>> ");
        }
    }
}
