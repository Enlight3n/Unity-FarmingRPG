using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>
{
    private int gameYear = 1;
    private Season gameSeason = Season.Spring;
    private int gameDay = 1;
    private int gameHour = 6;
    private int gameMinute = 30;
    private int gameSecond = 0;
    private string gameDayOfWeek = "Mon";
    private bool gameClockPaused = false;
    private float gameTick = 0f;

    private void Start()
    {
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute,
            gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)
        {
            GameTick();
        } 
    }

    private void GameTick()
    {
        gameTick += Time.deltaTime;

        if (gameTick >= Settings.secondsPerGameSecond)
        {
            gameTick -= Settings.secondsPerGameSecond;

            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;

        if (gameSecond > 59)
        {
            gameSecond = 0;
            gameMinute++;

            if (gameMinute > 59)
            {
                gameMinute = 0;
                gameHour++;
                if (gameHour > 23)
                {
                    gameHour = 0;
                    gameDay++;
                    if (gameDay > 30)
                    {
                        gameDay = 1;

                        //无异于gameSeason++
                        int gs = (int)gameSeason;
                        gs++;
                        gameSeason = (Season)gs;

                        if (gs > 3)
                        {
                            gs = 0;
                            gameSeason = (Season)gs;
                            
                            gameYear++;

                            if (gameYear > 9999)
                                gameYear = 1;
                            

                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek,
                                gameHour, gameMinute, gameSecond);
                        }

                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour,
                            gameMinute, gameSecond);
                    }
                    
                    gameDayOfWeek = GetDayOfWeek(gameDayOfWeek);

                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute,
                    gameSecond);
                }
                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute,
                    gameSecond);
            }
            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute,
                gameSecond);
            
        }
        /*Debug.Log("GameYear:" + gameYear + "    GameSeason:" + gameSeason + "    GameDay:" + gameDay +
                  "    GameDayOfWeek:" + gameDayOfWeek + "    GameHour:" + gameHour + "    GameMinute:" +
                  gameMinute +
                  "    GameSecond" + gameSecond);*/
    }

    //这个函数用来识别今天是周几，但是弊端在于会默认每年第一天为周一
    private string GetDayOfWeek()
    {
        int totalDays = (((int)gameSeason) * 30) + gameDay;

        int dayOfWeek = totalDays % 7;

        switch (dayOfWeek)
        {
            case 1:
                return "Monday";
            case 2:
                return "Tue";
            case 3:
                return "Wed";
            case 4:
                return "Thu";
            case 5:
                return "Fri";
            case 6:
                return "Sat";
            case 0:
                return "Sun";
            default:
                return "";
        }

    }

    //我认为这个用来识别周几函数更好
    private string GetDayOfWeek(string gamedayofweek)
    {
        switch (gamedayofweek)
        {
            case "Sun":
                return "Mon";
            case "Mon":
                return "Tue";
            case "Tue":
                return "Wed";
            case "Wed":
                return "Thu";
            case "Thu":
                return "Fri";
            case "Fri":
                return "Sat";
            case "Sat":
                return "Sun";
            default:
                return "";
        }
    }

    #region 开发者函数

    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }
    
    public void TestAdvanceGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            UpdateGameSecond();
        }
    }
    
    
    
    #endregion
}
