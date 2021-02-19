using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicCards
{
    public delegate void MagicHandler(MagicCard src_card);
    public delegate void MineCardHandler(MineCard src_card);
    public delegate void PlayerAIHandler(Player player, MagicOpportunity stage);
    public enum GemstoneColor { Red, Blue, Purple, Black, White, Mori }
    public enum MagicOpportunity { OnGemstone, OnMagicEffect, OnMagic, OnMineEffect, OnGetGemstone,
                                   OnGetMagic, OnGetMine, None }
    public struct GemstoneRequirement
    {
        public int[] need;
        public GemstoneRequirement(int red, int blue, int purple, int black, int white, int mori = 0)
        {
            need = new int[] { red,blue,purple,black,white,mori };
        }
        public bool Accepted(int[] gemstones)
        {
            for (int i = 0; i < 6; i++)
                if (gemstones[i] < need[i]) 
                    return false;
            return true;
        }
    }
    public struct MineCard
    {
        public string Name;
        public MineCardHandler Handler;
        public MineCard(string name, MineCardHandler handler) { Name = name; Handler = handler; }
    }
    public class MagicCard
    {
        public string Name;
        public int Level;
        public MagicHandler Handler;
        public object Appendix;
        public GemstoneRequirement Require;
        public MagicOpportunity Opportunity;
        public MagicCard(string name,MagicHandler handler, GemstoneRequirement require, MagicOpportunity op = MagicOpportunity.None)
        {
            Name = name; Handler = handler; Opportunity = op;
            Level = 0; Appendix = null; Require = require;
        }
        public void Learn(Player player)
        {
            Level++;
            for (int i = 0; i < 6; i++)
                player.gemstones[i] -= Require.need[i];
        }
        public bool IsLearn { get { return Level > 0; } }
    }
    public class Player
    {
        public int[] gemstones;
        public List<MagicCard> magics;
        public List<MineCard> mines;
        public PlayerAIHandler AI;
        public string name;
        public Player(string Name, PlayerAIHandler ai = null)
        {
            gemstones = new int[6];
            magics = new List<MagicCard>();
            mines = new List<MineCard>();
            name = Name;
            AI = ai;
        }
        public int Level{ get{ return magics.FindAll(m => m.Name == "等级卡").Count; } }
    }
    class Program
    {
        public static string[] GemstoneName = new string[] { "赤", "蓝", "紫", "黑", "白", "森" };
        static List<Player> playerLibrary = new List<Player>();
        static List<Player> players = new List<Player>();
        public static Player GetPlayer(string Name)
        {
            Player src = playerLibrary.Find(m => m.name == Name);
            Player p = new Player(src.name,src.AI);
            return p;
        }
        public async static Task RandomGemstone()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            foreach (Player p in players)
            {
                int ran = random.Next(0, 6);
                await Task.Run(() =>
                {
                    Console.WriteLine("这里执行骰子的动画...");
                    Console.WriteLine($"好欸玩家{p.name}掷到了{GemstoneName[ran]}宝石卡！");
                    Thread.Sleep(1000);
                });
                p.gemstones[ran]++;
            }
        }
        public async static void Game()
        {
            // 胜利条件
            while (players.FindIndex(m => m.Level >= 15) == -1)
            {
                // 骰子
                await RandomGemstone();
            }
        }
        static void Main(string[] args)
        {
            // 建立玩家
            playerLibrary.Add(new Player("玩家"));
            playerLibrary.Add(new Player("黑嘴",(p,s) =>
            {
                if(s == MagicOpportunity.OnMagic)
                {
                    // 好欸，当然是乱玩了
                    foreach (MagicCard mc in p.magics)
                        if (mc.Require.Accepted(p.gemstones))
                            mc.Learn(p);
                }
                if (s == MagicOpportunity.OnMagicEffect)
                {
                    // 好欸，我一只狗怎么懂得反击魔法效果呢，不处理
                }
                if (s == MagicOpportunity.OnMineEffect)
                {
                    // 好欸，我一只狗怎么懂得反击宝藏效果呢，不处理
                }
            }));

            // 输入玩家
            players.Add(GetPlayer("玩家"));
            players.Add(GetPlayer("黑嘴"));

            Game();

            Console.ReadLine();
        }
    }
}
