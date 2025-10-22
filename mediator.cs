using System;
using System.Collections.Generic;

namespace MediatorChatRoom
{
    public interface IMediator
    {
        void Register(User user);
        void Unregister(string userName);
        void SendMessage(string from, string message);              
        void SendPrivateMessage(string from, string to, string msg); 
        IReadOnlyCollection<string> GetUsers();
    }

    public class ChatRoom : IMediator
    {
        private readonly Dictionary<string, User> _users = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        public void Register(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (_users.ContainsKey(user.Name))
            {
                Console.WriteLine($"Пользователь с именем '{user.Name}' уже зарегистрирован.");
                return;
            }

            _users[user.Name] = user;
            user.SetMediator(this);
            BroadcastSystemMessage($"Пользователь '{user.Name}' присоединился к чату.", except: user.Name);
            Console.WriteLine($"Пользователь '{user.Name}' успешно зарегистрирован.");
        }

        public void Unregister(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return;
            if (!_users.ContainsKey(userName))
            {
                Console.WriteLine($"Нельзя удалить: пользователь '{userName}' не найден.");
                return;
            }

            _users.Remove(userName);
            BroadcastSystemMessage($"Пользователь '{userName}' покинул чат.");
            Console.WriteLine($"Пользователь '{userName}' удалён из чата.");
        }

        public void SendMessage(string from, string message)
        {
            if (!ValidateSender(from)) return;
            if (message == null) message = string.Empty;

            foreach (var u in _users.Values)
            {
                if (!u.Name.Equals(from, StringComparison.OrdinalIgnoreCase))
                    u.Receive(from, message, isPrivate: false);
            }
        }

        public void SendPrivateMessage(string from, string to, string msg)
        {
            if (!ValidateSender(from)) return;
            if (string.IsNullOrWhiteSpace(to))
            {
                Console.WriteLine("Укажите получателя приватного сообщения.");
                return;
            }
            if (!_users.ContainsKey(to))
            {
                Console.WriteLine($"Ошибка: получатель '{to}' не найден.");
                return;
            }
            var receiver = _users[to];
            receiver.Receive(from, msg, isPrivate: true);
        }

        private bool ValidateSender(string from)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                Console.WriteLine("Отправитель не указан.");
                return false;
            }
            if (!_users.ContainsKey(from))
            {
                Console.WriteLine($"Ошибка: отправитель '{from}' не зарегистрирован в чате.");
                return false;
            }
            return true;
        }

        private void BroadcastSystemMessage(string text, string except = null)
        {
            foreach (var u in _users.Values)
            {
                if (!u.Name.Equals(except, StringComparison.OrdinalIgnoreCase))
                    u.Receive("[Система]", text, isPrivate: false);
            }
        }

        public IReadOnlyCollection<string> GetUsers()
        {
            return _users.Keys;
        }
    }

    public class User
    {
        public string Name { get; }
        private IMediator _mediator;

        public User(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Имя не может быть пустым.", nameof(name));
            Name = name;
        }

        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void Send(string message)
        {
            if (_mediator == null)
            {
                Console.WriteLine($"[{Name}] Вы не в чате. Сначала зарегистрируйтесь.");
                return;
            }
            _mediator.SendMessage(Name, message);
        }

        public void SendPrivate(string to, string message)
        {
            if (_mediator == null)
            {
                Console.WriteLine($"[{Name}] Вы не в чате. Сначала зарегистрируйтесь.");
                return;
            }
            _mediator.SendPrivateMessage(Name, to, message);
        }

        public virtual void Receive(string from, string message, bool isPrivate)
        {
            if (isPrivate)
                Console.WriteLine($"[Приватно -> {Name}] от {from}: {message}");
            else
                Console.WriteLine($"[{Name}] Получено от {from}: {message}");
        }
    }

    public class AdminUser : User
    {
        public AdminUser(string name) : base(name) { }

        public void Announce(IMediator mediator, string announcement)
        {
            if (mediator == null) throw new ArgumentNullException(nameof(mediator));
            mediator.SendMessage(this.Name, $"[Объявление администратора] {announcement}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Mediator Pattern");
            var chat = new ChatRoom();

            while (true)
            {
                PrintMenu();
                var cmd = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(cmd)) { Console.WriteLine("Введите команду."); continue; }
                if (cmd == "0") break;

                switch (cmd)
                {
                    case "1":
                        RegisterUser(chat);
                        break;
                    case "2":
                        UnregisterUser(chat);
                        break;
                    case "3":
                        SendPublicMessage(chat);
                        break;
                    case "4":
                        SendPrivateMessage(chat);
                        break;
                    case "5":
                        ListUsers(chat);
                        break;
                    case "6":
                        RegisterAdmin(chat);
                        break;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }

            Console.WriteLine("Выход. Пока!");
        }

        static void PrintMenu()
        {
            Console.WriteLine("\nДоступные действия:");
            Console.WriteLine("1 - Зарегистрировать пользователя");
            Console.WriteLine("2 - Удалить пользователя");
            Console.WriteLine("3 - Отправить публичное сообщение");
            Console.WriteLine("4 - Отправить приватное сообщение");
            Console.WriteLine("5 - Показать пользователей");
            Console.WriteLine("6 - Зарегистрировать администратора (Admin)");
            Console.WriteLine("0 - Выход");
            Console.Write(">>> ");
        }

        static void RegisterUser(ChatRoom chat)
        {
            Console.Write("Введите имя нового пользователя: ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Имя не может быть пустым."); return; }
            var user = new User(name);
            chat.Register(user);
        }

        static void RegisterAdmin(ChatRoom chat)
        {
            Console.Write("Введите имя администратора: ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Имя не может быть пустым."); return; }
            var admin = new AdminUser(name);
            chat.Register(admin);

            Console.Write("Сделать объявление сейчас? (y/n): ");
            var ans = Console.ReadLine()?.Trim().ToLower();
            if (ans == "y")
            {
                Console.Write("Текст объявления: ");
                var text = Console.ReadLine() ?? "";
                admin.Announce(chat, text);
            }
        }

        static void UnregisterUser(ChatRoom chat)
        {
            Console.Write("Введите имя пользователя для удаления: ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Имя не может быть пустым."); return; }
            chat.Unregister(name);
        }

        static void SendPublicMessage(ChatRoom chat)
        {
            Console.Write("От кого (имя): ");
            var from = Console.ReadLine()?.Trim();
            Console.Write("Сообщение: ");
            var msg = Console.ReadLine() ?? "";
            chat.SendMessage(from, msg);
        }

        static void SendPrivateMessage(ChatRoom chat)
        {
            Console.Write("От кого (имя): ");
            var from = Console.ReadLine()?.Trim();
            Console.Write("Кому (имя получателя): ");
            var to = Console.ReadLine()?.Trim();
            Console.Write("Сообщение: ");
            var msg = Console.ReadLine() ?? "";
            chat.SendPrivateMessage(from, to, msg);
        }

        static void ListUsers(ChatRoom chat)
        {
            var users = chat.GetUsers();
            Console.WriteLine("Пользователи в чате:");
            foreach (var u in users) Console.WriteLine(" - " + u);
        }
    }
}
