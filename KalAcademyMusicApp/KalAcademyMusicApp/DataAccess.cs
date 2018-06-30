﻿using KalAcademyMusicApp.Models;
using System.Threading.Tasks;

namespace KalAcademyMusicApp
{
    public static class DataAccess
    {
        public static Playlist InitializeFromFile(string fileName)
        {
            return new Helper().ReadDataJson(fileName);
        }
    }
}
