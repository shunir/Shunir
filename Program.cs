using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TextRPG.Items;

namespace TextRPG
{
    public static class Story //스토리 작성 가능
    {
        public static void Intro()
        {
            Console.Clear();
            Console.WriteLine("====================================");
            Console.WriteLine("\n당신은 평범한 삶을 살던 중...");
            Console.WriteLine("\n갑자기 눈부신 빛에 휩싸이며 이세계에 떨어졌습니다.");
            Console.WriteLine("\n눈에 갑자기 문제가 생겼는지 시야는 온통 검은색입니다.");
            Console.WriteLine("\n앞이 보이지 않는 상태에서 누군가의 목소리가 들립니다...");
            Console.WriteLine("\n'안녕? 나는 라피야. 이 세계를 여행할 수 있도록 도와줄게!'");
            Console.WriteLine("====================================");
            Console.WriteLine("\n[계속하려면 아무 키나 누르세요]");
            Console.ReadKey();
        }

    
   
    }

    class Program
    {

        static void Main()
        {
            Console.Title = "이세계 여행";
            Console.ForegroundColor = ConsoleColor.Green;

            Story.Intro();


            Console.WriteLine("\n이세계 여행을 도와줄 라피야!");
            Console.Write("\n이름을 입력해주세요: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
                name = "플레이어";

            Player player = new(name, "시민");
            Stage.GetSelect(player);
        }
    }

    public class Player
    {
        public string Name { get; set; }
        public string Job { get; set; }
        public int Level { get; set; } = 1;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int Hp { get; set; } = 100;
        public int MaxHp { get; set; } = 100;
        public int Exp { get; set; } = 0;
        public int MaxExp { get; set; } = 100;
        public int Gold { get; set; } = 1500;
        public List<Item> Inventory { get; set; } = new();

        public Player(string name, string job)
        {
            Name = name;
            Job = job;
        }

        public void Rest()
        {
            Hp = MaxHp;
            Console.WriteLine("\n휴식을 통해 체력을 회복했습니다.");
        }

        public void ShowStatus()
        {
            Console.Clear();
            Console.WriteLine($"\n이름: {Name}의 정보");
            Console.WriteLine($"\n공격력: {Attack}");
            Console.WriteLine($"\n방어력: {Defense}");
            Console.WriteLine($"\n체력: {Hp}/{MaxHp}");
            Console.WriteLine($"\n경험치: {Exp}/{MaxExp}");
            Console.WriteLine($"\n골드: {Gold}\n");
            Console.WriteLine("\n[장착된 아이템]");

            foreach (var item in Inventory)
            {
                if (item.Equipped)
                {
                    string itemType = item is Weapon ? "무기" : item is Armor ? "방어구" : "기타";
                    int attack = item is Weapon weapon ? weapon.Attack : 0;
                    int defense = item is Armor armor ? armor.Defense : 0;
                    Console.WriteLine($"{item.Name} - 종류: {itemType}, 공격력: {attack}, 방어력: {defense}");
                }
            }
        }

        public void ManageInventory()
        {
            Console.Clear();
            Console.WriteLine("[인벤토리]");
            for (int i = 0; i < Inventory.Count; i++)
            {
                var item = Inventory[i];
                string status = item.Equipped ? "(장착중)" : "";
                Console.WriteLine($"{i + 1}. {item.Name} {status}");
            }
            Console.Write("\n장착/해제할 아이템 번호 \n(0: 나가기): ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= Inventory.Count)
            {
                var selected = Inventory[index - 1];
                if (selected.Equipped)
                {
                    selected.Equipped = false;
                    if (selected is Weapon weapon)
                        Attack -= weapon.Attack;
                    else if (selected is Armor armor)
                        Defense -= armor.Defense;
                }
                else
                {
                    selected.Equipped = true;
                    if (selected is Weapon weapon)
                        Attack += weapon.Attack;
                    else if (selected is Armor armor)
                        Defense += armor.Defense;
                }
                Console.WriteLine($"{selected.Name} {(selected.Equipped ? "장착" : "해제")} 완료");
            }
        }
    }

    public static class Stage
    {
        public static void GetSelect(Player p)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n어떤걸 하고 싶어? 선택해줘!");
                Console.WriteLine("\n- 상태\n- 인벤토리\n- 상점\n- 던전\n- 휴식\n- 저장\n- 불러오기\n- 종료");
                Console.Write("입력: ");
                string command = Console.ReadLine().Trim().ToLower();

                switch (command)
                {
                    case "상태": p.ShowStatus(); break;
                    case "인벤토리": p.ManageInventory(); break;
                    case "상점": Shop.Enter(p); break;
                    case "던전": Dungeon.Enter(p); break;
                    case "휴식": p.Rest(); break;
                    case "저장": Save(p); break;
                    case "불러오기": p = Load(); break;
                    case "종료": return;
                    default: Console.WriteLine("잘못된 명령어입니다."); break;
                }
                Console.ReadKey();
            }
        }

        static void Save(Player player)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };  
            string json = JsonSerializer.Serialize(player, options);
            File.WriteAllText("save.json", json);
            Console.WriteLine("\n게임이 저장되었습니다.");
        }

        static Player Load()
        {
            if (!File.Exists("save.json"))
            {
                Console.WriteLine("\n저장 파일이 없습니다.");
                return new Player("\n불러오기 실패", "시민");
            }
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            string json = File.ReadAllText("save.json");
            try
            {
                var player = JsonSerializer.Deserialize<Player>(json, options);
                Console.WriteLine("\n게임을 불러왔습니다.");
                return player ?? new Player("\n불러오기 실패", "시민");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n불러오기 중 오류 발생: {ex.Message}");
                return new Player("\n불러오기 실패", "시민");
            }
        }
    }

    public static class Shop
    {
        static List<Item> items = new()
        {
            new Weapon("나무검", 100, "기본적인 나무로 만든 검", 5),
            new Weapon("철검", 300, "단단한 철로 만든 검", 10),
            new Armor("가죽 방어구", 200, "가죽으로 만든 방어구", 5),
            new Armor("강철 갑옷", 500, "강철로 된 튼튼한 갑옷", 10)
        };

        public static void Enter(Player p)
        {
            Console.WriteLine("[상점]");
            for (int i = 0; i < items.Count; i++)
                Console.WriteLine($"{i + 1}. {items[i].Name} - {items[i].Description} ({items[i].Price} G)");

            Console.Write("\n구매할 아이템 번호 입력 \n(0: 나가기): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= items.Count)
            {
                var item = items[choice - 1];
                if (p.Gold >= item.Price)
                {
                    p.Gold -= item.Price;
                    // 아이템 복제하여 인벤토리에 추가 (참조 공유 방지)
                    if (item is Weapon w)
                        p.Inventory.Add(new Weapon(w.Name, w.Price, w.Description, w.Attack));
                    else if (item is Armor a)
                        p.Inventory.Add(new Armor(a.Name, a.Price, a.Description, a.Defense));
                    else
                        p.Inventory.Add(item);
                    Console.WriteLine($"\n{item.Name} 구매 완료!");
                }
                else Console.WriteLine("\n골드가 부족합니다.");
            }
        }
    }

    public static class Dungeon
    {
        // 상점 아이템 목록을 Dungeon 클래스에 추가하여 오류 해결
        static List<Item> items = new()
        {
            new Weapon("나무 몽둥이", 50, "던전에서 발견된 나무 몽둥이", 3),
            new Weapon("돌도끼", 200, "던전에서 발견된 돌로 만든 도끼", 8),
            new Armor("낡은 갑옷", 120, "오래된 갑옷", 4),
            new Armor("미스릴 갑옷", 800, "희귀한 미스릴로 만든 갑옷", 15)
        };

        public static void Enter(Player p)
        {
            Console.WriteLine("\n[던전 상점]");
            for (int i = 0; i < items.Count; i++)
                Console.WriteLine($"{i + 1}. {items[i].Name} - {items[i].Description} ({items[i].Price} G)");

            Console.Write("\n구매할 아이템 번호 입력 \n(0: 나가기): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= items.Count)
            {
                var item = items[choice - 1];
                if (p.Gold >= item.Price)
                {
                    p.Gold -= item.Price;
                    // 아이템 복제하여 인벤토리에 추가 (참조 공유 방지)
                    if (item is Weapon w)
                        p.Inventory.Add(new Weapon(w.Name, w.Price, w.Description, w.Attack));
                    else if (item is Armor a)
                        p.Inventory.Add(new Armor(a.Name, a.Price, a.Description, a.Defense));
                    else
                        p.Inventory.Add(item);
                    Console.WriteLine($"\n{item.Name} 구매 완료!");
                }
                else Console.WriteLine("\n골드가 부족합니다.");
            }
        }
    }
}

namespace TextRPG.Entities
{
    public class Monster
    {
        public string Name { get; }
        public int Hp { get; set; }
        public int Attack { get; }

        public Monster(string name, int hp, int attack)
        {
            Name = name;
            Hp = hp;
            Attack = attack;
        }
    }
}

namespace TextRPG.Items
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Weapon), typeDiscriminator: "weapon")]
    [JsonDerivedType(typeof(Armor), typeDiscriminator: "armor")]
    public abstract class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public bool Equipped { get; set; }
        public abstract void Use(TextRPG.Player p);
    }

    public class Weapon : Item
    {
        public int Attack { get; set; }
        public Weapon(string name, int price, string desc, int attack)
        {
            Name = name;
            Price = price;
            Description = desc;
            Attack = attack;
        }
        public override void Use(TextRPG.Player p)
        {
            p.Attack += Attack;
            Equipped = true;
            Console.WriteLine($"\n{Name}을 장착했습니다.");
        }
    }

    public class Armor : Item
    {
        public int Defense { get; set; }
        public Armor(string name, int price, string desc, int defense)
        {
            Name = name;
            Price = price;
            Description = desc;
            Defense = defense;
        }
        public override void Use(TextRPG.Player p)
        {
            p.Defense += Defense;
            Equipped = true;
            Console.WriteLine($"\n{Name}을 장착했습니다.");
        }
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Quest
    }
}
