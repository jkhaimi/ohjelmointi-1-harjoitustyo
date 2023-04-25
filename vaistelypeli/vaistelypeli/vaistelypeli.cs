using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author jkhaimi @version 25.11.2021
/// <summary>
/// Väistelypeli
/// debug näyte, animaatiot ja vihu aliohjelmat samaan
/// </summary>


public class Väistelypeli : PhysicsGame
{



    private static String[] lines = {




                    "s           V               S",
                    "s                           S",
                    "s                           S",
                    "s                           S",
                    "s                           S",
                    "s                           S",
                    "s                           S",
                    "s                           S",
                    "s             P             S",
                    "s v                       w S",
                    "MMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
                    };

    private static int tileWidth = 800 / lines[0].Length;
    private static int tileHeight = 480 / lines.Length;
    private TileMap tiles = TileMap.FromStringArray(lines);

    private PlatformCharacter pelaaja;
    private Image pelaajanKuva = LoadImage("pelaaja.png");
    private Image vihu1Kuva = LoadImage("kakka.png");
    private Image vihu2Kuva = LoadImage("muta.png");

    private SoundEffect Kuolema = LoadSoundEffect("kuolema");

    private PhysicsObject vihu;
    private PhysicsObject vihu2;
    private PhysicsObject vihu3;
    private Surface maa;
    private Surface seina;

    private Timer ajastin;
    private EasyHighScore topLista = new EasyHighScore();

    private const double nopeus = 200;
    private const double hyppynopeus = 400;
    private Vector vihunopeus = new Vector(0, -300);
    private Vector vihunopeus2 = new Vector(300, 0);
    private Vector vihunopeus3 = new Vector(-300, 0);


    public override void Begin()
    {



        Gravity = new Vector(0, -750);
        Level.BackgroundColor = Color.White;

        tiles.SetTileMethod('P', LisaaPelaaja);
        tiles.SetTileMethod('V', LisaaVihu);
        tiles.SetTileMethod('v', LisaaVihu2);
        tiles.SetTileMethod('w', LisaaVihu3);
        tiles.SetTileMethod('M', LisaaMaa);
        tiles.SetTileMethod('S', LisaaSeina, "Seina");
        tiles.SetTileMethod('s', LisaaSeina, "Seina");

        tiles.Execute(tileWidth, tileHeight);

        Camera.ZoomToLevel();
        Camera.ZoomFactor = 1.4;

        MediaPlayer.Play("ht");

        LisaaOhjaimet();
        Ajastin();


    }

    /// <summary>
    /// Lisätään maa
    /// </summary>
    /// <param name="paikka">Paikka mihin maa luodaan</param>
    /// <param name="leveys">Maan leveys</param>
    /// <param name="korkeus">Maan korkeus</param>
    private void LisaaMaa(Vector paikka, double leveys, double korkeus)
    {
        maa = new Surface(leveys, korkeus);
        maa.Position = paikka;
        maa.Shape = Shape.Rectangle;
        maa.Color = Color.Black;
        maa.IgnoresGravity = true;
        maa.Tag = "maa";
        Add(maa);
    }


    /// <summary>
    /// Lisätään seinä
    /// </summary>
    /// <param name="paikka">Paikka mihin seinä luodaan</param>
    /// <param name="leveys">Seinän leveys</param>
    /// <param name="korkeus">Seinän korkeus</param>
    /// <param name="tagi">Seinän nimi jolla tunnistetaan osumat seinään</param>
    private void LisaaSeina(Vector paikka, double leveys, double korkeus, string tagi)
    {
        seina = new Surface(leveys, korkeus);
        seina.Position = paikka;
        seina.Shape = Shape.Rectangle;
        seina.Color = Color.Red;
        seina.IgnoresGravity = true;
        seina.IsVisible = false;
        seina.Tag = tagi;
        Add(seina);
    }


    /// <summary>
    /// Lisätään pelaaja
    /// </summary>
    /// <param name="paikka">Paikka mihin pelaaja luodaan</param>
    /// <param name="leveys">Pelaajan leveys</param>
    /// <param name="korkeus">Pelaajan korkeus</param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja = new PlatformCharacter(leveys * 2, korkeus * 1.0);
        pelaaja.Position = paikka;
        pelaaja.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja, "vihu", PeliLoppuu);
        AddCollisionHandler(pelaaja, "vihu2", PeliLoppuu);
        AddCollisionHandler(pelaaja, "vihu3", PeliLoppuu);
        Add(pelaaja);
    }


    /// <summary>
    /// Lisätään alaspäin laskeva vihollinen
    /// </summary>
    /// <param name="paikka">Paikka mihin vihollinen luodaan</param>
    /// <param name="leveys">Vihollisen leveys</param>
    /// <param name="korkeus">Vihollisen korkeus</param>
    private void LisaaVihu(Vector paikka, double leveys, double korkeus)
    {
        vihu = new PhysicsObject(leveys * 1.5, korkeus);
        vihu.Position = paikka;
        vihu.Image = vihu1Kuva;
        vihu.Shape = Shape.Circle;
        vihu.Color = Color.Brown;
        vihu.IgnoresGravity = true;
        vihu.Velocity = vihunopeus;
        vihu.Tag = "vihu";
        vihu.CollisionIgnoreGroup = 1;
        vihu2.CollisionIgnoreGroup = 1;
        vihu3.CollisionIgnoreGroup = 1;
        AddCollisionHandler(vihu, "maa", VihuTormaa);
        Add(vihu);
    }


    /// <summary>
    /// Lisätään ensimmäinen sivuttaissuuntainen vihollinen
    /// </summary>
    /// <param name="paikka">Paikka mihin vihollinen luodaan</param>
    /// <param name="leveys">Vihollisen leveys</param>
    /// <param name="korkeus">Vihollisen korkeus</param>
    private void LisaaVihu2(Vector paikka, double leveys, double korkeus)
    {
        vihu2 = new PhysicsObject(leveys * 1.5, korkeus);
        vihu2.Position = paikka;
        vihu2.Image = vihu2Kuva;
        vihu2.AngularVelocity = -1;
        vihu2.Shape = Shape.Circle;
        vihu2.Color = Color.Brown;
        vihu2.IgnoresGravity = true;
        vihu2.Velocity = vihunopeus2;
        vihu2.Tag = "vihu2";
        AddCollisionHandler(vihu2, "Seina", VihuTormaaO);
        ///AddCollisionHandler(vihu2, "vSeina", VihuTormaaV);
        Add(vihu2);
    }


    /// <summary>
    /// Lisätään toinen sivuttaissuuntainen vihollinen
    /// </summary>
    /// <param name="paikka">Paikka mihin vihollinen luodaan</param>
    /// <param name="leveys">Vihollisen leveys</param>
    /// <param name="korkeus">Vihollisen korkeus</param>
    private void LisaaVihu3(Vector paikka, double leveys, double korkeus)
    {
        vihu3 = new PhysicsObject(leveys * 1.5, korkeus);
        vihu3.Position = paikka;
        vihu3.Image = vihu2Kuva;
        vihu3.AngularVelocity = 1;
        vihu3.Shape = Shape.Circle;
        vihu3.Color = Color.Brown;
        vihu3.IgnoresGravity = true;
        vihu3.Velocity = vihunopeus3;
        vihu3.Tag = "vihu3";

        Add(vihu3);
    }


    /// <summary>
    /// Luodaan aliohjelma pelin loppumista varten
    /// </summary>
    /// <param name="tormaaja">Pelaaja joka törmää</param>
    /// <param name="kohde">Kohde mihin pelaaja törmää</param>
    private void PeliLoppuu(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        pelaaja.Destroy();
        vihu.Destroy();
        vihu2.Destroy();
        vihu3.Destroy();
        MessageDisplay.Add("Sotkit kengät");
        IsPaused = true;
        MediaPlayer.Pause();
        Kuolema.Play();

        Loppuvalikko();
    }


    /// <summary>
    /// Ajastin, jota käytetään pisteiden laskemiseen
    /// </summary>
    private void Ajastin()
    {
        Timer ajastin = new Timer();
        ajastin.Start();

        Label aikanaytto = new Label();
        aikanaytto.TextColor = Color.Black;
        aikanaytto.DecimalPlaces = 1;
        aikanaytto.X = 0;
        aikanaytto.Y = Level.Top - 50;
        aikanaytto.BindTo(ajastin.SecondCounter);

        Add(aikanaytto);

    }


    /// <summary>
    /// Ensimmäinen vihollinen osuu maahan
    /// </summary>
    /// <param name="tormaaja">vihollinen joka osuu maahan</param>
    /// <param name="kohde">Maa johon vihollinen osuu</param>
    private void VihuTormaa(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        vihu.Destroy();

        int luku = RandomGen.NextInt(-350, 350);
        Vector paikka = new Vector(luku, 300);
        LisaaVihu(paikka, 27, 42);
    }


    /// <summary>
    /// Sivuttaissuuntainen vihollinen osuu oikeaan seinään
    /// </summary>
    /// <param name="tormaaja">Vihollinen joka osuu seinään</param>
    /// <param name="kohde">Seinä johon vihollinen osuu</param>
    private void VihuTormaaO(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        vihu2.Velocity = -vihunopeus2;
        vihu3.Velocity = -vihunopeus3;
    }


    /// <summary>
    /// Sivuttaissuuntainen vihollinen osuu vasempaan seinään
    /// </summary>
    /// <param name="tormaaja">Vihollinen joka osuu seinään</param>
    /// <param name="kohde">Seinä johon vihollinen osuu</param>
    private void VihuTormaaV(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        vihu2.Velocity = vihunopeus2;
        vihu3.Velocity = vihunopeus3;
    }


    /// <summary>
    /// Lisätään ohjaimet peliin
    /// </summary>
    private void LisaaOhjaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liiku, "Pelaaja liikkuu vasemmalle", pelaaja, -nopeus);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liiku, "Pelaaja liikkuu oikealle", pelaaja, nopeus);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja, hyppynopeus);
    }


    /// <summary>
    /// Lisätään pelaajalle liikettä
    /// </summary>
    /// <param name="pelaaja">pelaaja joka liikkuu</param>
    /// <param name="nopeus">pelaajan nopeus</param>
    private void Liiku(PlatformCharacter pelaaja, double nopeus)
    {
        pelaaja.Walk(nopeus);
    }


    /// <summary>
    /// Lisätään pelaajalle hyppy
    /// </summary>
    /// <param name="pelaaja">pelaaja joka hyppää</param>
    /// <param name="hyppynopeus">pelaajan hyppynopeus</param>
    private void Hyppaa(PlatformCharacter pelaaja, double hyppynopeus)
    {
        pelaaja.Jump(hyppynopeus);
    }


    /// <summary>
    /// Lisätään loppuvalikko pelin päätyttyä
    /// </summary>
    private void Loppuvalikko()
    {
        MultiSelectWindow loppuvalikko = new MultiSelectWindow("Game Over", "Pelaa uudestaan", "Jaa pisteet", "Lopeta");
        Add(loppuvalikko);
        loppuvalikko.AddItemHandler(0, AloitaPeli);
        loppuvalikko.AddItemHandler(1, Lista, ajastin);
        loppuvalikko.AddItemHandler(2, ConfirmExit);
    }


    /// <summary>
    /// Kun halutaan aloittaa peli alusta
    /// </summary>
    private void AloitaPeli()
    {
        ClearAll();
        Begin();

    }


    /// <summary>
    /// Kun halutaan lisätä omat pisteet listalle
    /// </summary>
    /// <param name="ajastin">ajastin, josta saadaan pisteet, eli selvitty aika</param>
    private void Lista(Timer ajastin)
    {
        double aikaaKulunut = ajastin.SecondCounter.Value;
        topLista.EnterAndShow(aikaaKulunut);
        topLista.HighScoreWindow.Closed += TaulukonJälkeen;
    }


    /// <summary>
    /// Pisteiden lisäämisen jälkeen peli alkaa uudestaan
    /// </summary>
    /// <param name="sender">peli avaa uuden ikkunan</param>
    private void TaulukonJälkeen(Window sender)
    {
        ClearAll();
        Begin();
    }
}

