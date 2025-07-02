using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ClickableTransparentOverlay;
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using System.Net;

class Program
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_MINIMIZE = 6;
    const int Ver = 1;

    static UI ui = new UI();
    static async Task Main()
    {
        Console.Title = "svv";
        Console.Clear();
        LogoAs();

        Thread.Sleep(1500);

        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_MINIMIZE);

        Task.Run(Update);

        Task.Run(LoadStuff);

        ui.Start().Wait();
    }

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);
    public static bool KeyDown(int vKey)
    {
        return GetAsyncKeyState(vKey) < 0;
    }

    static public bool IsRC = false;

    static int OldTab = 1;
    static void Update()
    {
        while (true)
        {
            if (KeyDown(0xA3))
            {
                if (!IsRC)
                {
                    UI.Enabled = !UI.Enabled;
                    Console.WriteLine("svv toggled");
                    IsRC = true;
                }
            }
            else IsRC = false;

            if (OldTab != UI.Catagorie)
            {
                OldTab = UI.Catagorie;
                Console.WriteLine($"cat {UI.Catagorie} entered");
            }

            Thread.Sleep(5);
        }
    }

    static void LogoAs()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"     _________  _____  __
    /  ___/\  \/ /\  \/ /
     \___ \  \   /  \   /
    /____  >  \_/    \_/
         \/");
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static bool Loaddd = false;
    public static async Task LoadStuff()
    {
        while (!Loaddd)
        {
            await Task.Delay(100);
        }
        Console.WriteLine("loading svv saves");
        Items.Load();
        UI.LoadingWhat = "[Online Data]";
        Console.WriteLine("loading online data");

        string Data = "";
        try
        {
            WebRequest request = WebRequest.Create("https://raw.githubusercontent.com/sevisadev/svv/refs/heads/main/Cheats.txt" + "?q=" + DateTime.UtcNow.Ticks);
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string html = "";
            using (StreamReader sr = new StreamReader(data))
            {
                html = sr.ReadToEnd();
            }
            Data = html;
        }
        catch (Exception ex)
        {
            UI.LoadingWhat = "[ERROR, Check Console]";
            Console.WriteLine($"Error loading online data:\n\n{ex}");
        }

        await Task.Delay(200);
        UI.LoadingWhat = "[Decompiling Data]";
        Console.WriteLine("decompiling data");

        string[] q = Data.Split(">");
        foreach (string line in q)
        {
            if (line.Contains(";"))
            {
                string[] parts = line.Split(';');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    if (!Items.Cheats.ContainsKey(key))
                    {
                        Items.Cheats.Add(key, value);
                    }
                }
            }
        }

        await Task.Delay(200);
        UI.LoadingWhat = "[UI]";
        await Task.Delay(500);
        Console.WriteLine("loading UI");
        UI.loadingDone = true;
    }
}

class Items
{
    public static List<bool> HoldButt = new List<bool>();
    public static List<bool> ItemOn = new List<bool>();
    public static Vector4 ThemeColor = new Vector4(0.8f, 0f, 0.8f, 1f);

    public static Dictionary<string, string> Cheats = new Dictionary<string, string>();

    static string SavePath => Path.Combine("C:/ProgramData", "svv_settings.dat");

    public static void SetHeld(int Index, bool State)
    {
        while (HoldButt.Count <= Index) HoldButt.Add(false);
        HoldButt[Index] = State;
    }

    public static bool GetHeld(int Index)
    {
        while (HoldButt.Count <= Index) HoldButt.Add(false);
        return HoldButt[Index];
    }

    public static string GetOnString(int Index, bool Def)
    {
        while (ItemOn.Count <= Index) ItemOn.Add(Def);
        return ItemOn[Index] ? "On" : "Off";
    }

    public static bool GetOnBool(int Index, bool Def)
    {
        while (ItemOn.Count <= Index) ItemOn.Add(Def);
        return ItemOn[Index];
    }

    public static void SetOn(int Index, bool State)
    {
        while (ItemOn.Count <= Index) ItemOn.Add(false);
        ItemOn[Index] = State;
        Save();
    }

    public static void Save()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(SavePath, FileMode.Create)))
        {
            writer.Write(ItemOn.Count);
            foreach (bool b in ItemOn) writer.Write(b);
            writer.Write(ThemeColor.X);
            writer.Write(ThemeColor.Y);
            writer.Write(ThemeColor.Z);
            writer.Write(ThemeColor.W);
        }
    }

    public static void Load()
    {
        if (!File.Exists(SavePath)) return;
        using (BinaryReader reader = new BinaryReader(File.Open(SavePath, FileMode.Open)))
        {
            int count = reader.ReadInt32();
            ItemOn.Clear();
            for (int i = 0; i < count; i++) ItemOn.Add(reader.ReadBoolean());
            ThemeColor = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}

class UI : Overlay
{
    public static int Catagorie = 1;
    public static bool Enabled = true;
    public static bool Loaded = false;
    private float fadeAlpha = 0f;
    private float timer = 0f;
    public static bool loadingDone = false;
    public static string LoadingWhat = "[Saves]";
    public static int CheatSel = 0;

    private float cheatFadeAlpha = 0f;
    private float cheatTimer = 0f;
    private bool cheatLoadingActive = false;

    protected override void Render()
    {
        var io = ImGui.GetIO();
        var drawList = ImGui.GetBackgroundDrawList();
        var screenSize = io.DisplaySize;

        if (!Loaded)
        {
            if (!loadingDone)
            {
                timer += io.DeltaTime;
                fadeAlpha = Math.Min(1f, timer / 1.0f);
                if (fadeAlpha >= 1f)
                {
                    Program.Loaddd = true;
                }
            }
            else
            {
                if (timer > 1.0f) timer = 0f;

                timer += io.DeltaTime;
                fadeAlpha = Math.Max(0f, 1f - (timer / 1.0f));

                if (fadeAlpha <= 0f)
                {
                    Loaded = true;
                }
            }


            uint black = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, fadeAlpha));
            drawList.AddRectFilled(Vector2.Zero, screenSize, black);

            if (fadeAlpha > 0.01f)
            {
                string loadingText = $"Loading...\n{LoadingWhat}";
                float scale = 2.5f;
                var textSize = ImGui.CalcTextSize(loadingText) * scale;
                var center = new Vector2((screenSize.X - textSize.X) / 2f, (screenSize.Y - textSize.Y) / 2f);
                drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize() * scale, center, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, fadeAlpha)), loadingText);
            }

            return;
        }

        if (Items.GetOnBool(0, false))
        {
            ImGui.Begin("crosshair_overlay", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground);
            var center = new Vector2(io.DisplaySize.X / 2f, io.DisplaySize.Y / 2f);
            float lineLength = 10f;
            uint color = ImGui.ColorConvertFloat4ToU32(Items.ThemeColor);
            drawList.AddLine(new Vector2(center.X - lineLength, center.Y), new Vector2(center.X + lineLength, center.Y), color, 2f);
            drawList.AddLine(new Vector2(center.X, center.Y - lineLength), new Vector2(center.X, center.Y + lineLength), color, 2f);
            ImGui.End();
        }

        if (!Enabled) return;

        var Style = ImGui.GetStyle();
        Vector2 windowSize = new Vector2(800, 400);

        Style.Colors[(int)ImGuiCol.WindowBg] = Items.ThemeColor * new Vector4(0.25f, 0.25f, 0.25f, 1f);
        Style.Colors[(int)ImGuiCol.ChildBg] = Items.ThemeColor * new Vector4(0.15f, 0.15f, 0.15f, 1f);
        Style.Colors[(int)ImGuiCol.Text] = Items.ThemeColor * new Vector4(0.6f, 0.6f, 0.6f, 1f);
        Style.Colors[(int)ImGuiCol.Border] = new Vector4(0, 0, 0, 0);
        Style.Colors[(int)ImGuiCol.Button] = Items.ThemeColor * new Vector4(0.25f, 0.25f, 0.25f, 0.4f);
        Style.Colors[(int)ImGuiCol.ButtonHovered] = Items.ThemeColor * new Vector4(0.3f, 0.3f, 0.3f, 0.5f);
        Style.Colors[(int)ImGuiCol.ButtonActive] = Items.ThemeColor * new Vector4(0.35f, 0.35f, 0.35f, 0.6f);
        Style.Colors[(int)ImGuiCol.FrameBg] = Items.ThemeColor * new Vector4(0.3f, 0.3f, 0.3f, 1f);
        Style.Colors[(int)ImGuiCol.FrameBgHovered] = Items.ThemeColor * new Vector4(0.4f, 0.4f, 0.4f, 1f);
        Style.Colors[(int)ImGuiCol.FrameBgActive] = Items.ThemeColor * new Vector4(0.5f, 0.5f, 0.5f, 1f);

        Style.WindowPadding = new Vector2(0, 0);
        Style.WindowRounding = 50;
        Style.ChildRounding = 50;
        Style.SeparatorTextBorderSize = 2f;
        Style.FrameRounding = 20;

        ImGui.Begin("svv", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
        ImGui.SetWindowSize(windowSize);
        ImGui.BeginChild("svv", new Vector2(200, 400));
        ImGui.Text(@"     _________  _____  __
    /  ___/\  \/ /\  \/ /
    \___ \  \   /  \   /
   /____  >  \_/    \_/
        \/");
        ImGui.SeparatorText("");
        ImGui.Indent(10);
        if (ImGui.Button("Main", new Vector2(180, 40))) Catagorie = 1;
        if (ImGui.Button("Overlays", new Vector2(180, 40))) Catagorie = 2;
        ImGui.EndChild();

        ImGui.SameLine();
        ImGui.BeginGroup();
        ImGui.Indent(40);
        switch (Catagorie)
        {
            case 1:
                ImGui.Text("\n\nMain");
                ImGui.Dummy(new Vector2(0, 60));
                ImGui.Text("Welcome to svv.\n\n\nsvv is a multi purpose menu, with many features.\n\n\nRight-Ctrl to toggle menu.\nX at top right to close menu.");
                ImGui.Text("\n\nMenu Theme:");
                if (ImGui.ColorEdit4("", ref Items.ThemeColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar))
                {
                    Items.Save();
                }
                break;
            case 2:
                ImGui.Text("\n\nOverlays");
                if (ImGui.Button($"Crosshair Overlay | {Items.GetOnString(0, false)}", new Vector2(180, 40)))
                {
                    if (!Items.GetHeld(0))
                    {
                        Items.SetOn(0, !Items.GetOnBool(0, true));
                        Items.SetHeld(0, true);
                    }
                }
                else Items.SetHeld(0, false);
                break;
        }
        ImGui.EndGroup();

        ImGui.SameLine();
        ImGui.BeginGroup();
        Vector2 closeButtonPos = new Vector2(windowSize.X - 25 - 50, 20);
        ImGui.SetCursorPos(closeButtonPos);
        if (ImGui.Button("x", new Vector2(25, 25))) Environment.Exit(0);
        ImGui.EndGroup();
        ImGui.End();
    }
}
