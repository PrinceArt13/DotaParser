using DotaParser.Models;
using DotaParser.Models.Entities;
using DotaParser.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DotaParser
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public ViewModel()
        {
            heroList = new();
            DataGridItem = new();
            IsEnableHeroDetails = false;
            IsEnableDeleteHero = false;
            IsEnableParsing = true;
            ProgressBarVisibility = "Hidden";
            ShablonPath = @"C:\Users\artem\Desktop\Архитектура ИС\privetVsem.doc";
            SaveAsPath = @"C:\Users\artem\Desktop\Архитектура ИС\8И11 Принцев АИС Разработка БД и механизмов наполненияdocx.docx";
        }

        private string progressBarVisibility;
        public string ProgressBarVisibility
        {
            get
            {
                return progressBarVisibility;
            }
            set
            {
                progressBarVisibility = value;
                OnPropertyChanged();
            }
        }

        private List<HeroVM> heroList;
        public List<HeroVM> HeroList
        {
            get
            {
                return heroList;
            }
            set
            {
                heroList = value;
                OnPropertyChanged();
            }
        }

        private Command getHeroes;
        public Command GetHeroes
        {
            get
            {
                return getHeroes ??= new Command(obj =>
                {
                    using (var db = new dbContext())
                    {
                        var HeroesInDB = db.Heroes
                                         .OrderBy(x => x.Name);

                        HeroList = HeroesInDB.Select(x => new HeroVM
                        {
                            Name = x.Name,
                            Mana = x.Mana,
                            Health = x.Health,
                            Armor = x.Armor,
                            MagicResistance = x.MagicResistance,
                            AttackType = x.AttackType,
                            Damage = x.Damage,
                            MoveSpeed = x.MoveSpeed
                        }).ToList();
                    }
                });
            }
        }

        private int dataGridIndex;
        public int DataGridIndex
        {
            get
            {
                return dataGridIndex;
            }
            set
            {
                dataGridIndex = value;
                OnPropertyChanged();
            }
        }

        private HeroVM dataGridItem;
        public HeroVM DataGridItem
        {
            get
            {
                return dataGridItem;
            }
            set
            {
                dataGridItem = value;
                OnPropertyChanged();
                if (dataGridItem != null)
                {
                    IsEnableDeleteHero = true;
                    IsEnableHeroDetails = true;
                }
                else
                {
                    IsEnableDeleteHero = false;
                    IsEnableHeroDetails = false;
                }
            }
        }

        private Hero heroDB;
        public Hero HeroDB
        {
            get
            {
                return heroDB;
            }
            set
            {
                heroDB = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> rolesDB;
        public ObservableCollection<string> RolesDB
        {
            get
            {
                return rolesDB;

            }
            set
            {
                rolesDB = value;
                OnPropertyChanged();
            }
        }

        private string mainAttributeDB;
        public string MainAttributeDB
        {
            get
            {
                return mainAttributeDB;
            }
            set
            {
                mainAttributeDB = value;
                OnPropertyChanged();
            }
        }

        private bool isEnableHeroDetails;
        public bool IsEnableHeroDetails
        {
            get
            {
                return isEnableHeroDetails;
            }
            set
            {
                isEnableHeroDetails = value;
                OnPropertyChanged();
            }
        }

        private bool attackType;
        public int AttackType
        {
            get
            {
                return Convert.ToInt32(attackType);
            }
            set
            {
                attackType = Convert.ToBoolean(value);
                OnPropertyChanged();
            }
        }

        private List<MainAttribute> GetAttributesList()
        {
            List<MainAttribute> attributesList = new();
            using (var db = new dbContext())
            {
                attributesList = db.MainAttributes.OrderBy(x=>x.Name).ToList();
            }
            return attributesList;
        }

        public List<string> AttributesList
        {
            get
            {
                return GetAttributesList().Select(x => x.Name).ToList();
            }
        }

        private List<Role> GetRolesList()
        {
            List<Role> rolesList = new();
            using (var db = new dbContext())
            {
                rolesList = db.Roles.OrderBy(x => x.Name).ToList();
            }
            return rolesList;
        }
        public List<string> RolesList
        {
            get
            {
                return GetRolesList().Select(x => x.Name).ToList();
            }
        }
        private Role? selectedRole;
        public string SelectedRole
        {
            get
            {
                if (selectedRole == null)
                    return "";
                else
                    return selectedRole.Name;
            }
            set
            {
                selectedRole = GetRolesList().Where(x => x.Name == value).FirstOrDefault();
                OnPropertyChanged();
            }
        }

        private string selectedRoleDB;
        public string SelectedRoleDB
        {
            get
            {
                return selectedRoleDB;
            }
            set
            {
                selectedRoleDB = value;
                OnPropertyChanged();
            }
        }

        private Command heroDetails;
        public Command HeroDetails
        {
            get
            {
                return heroDetails ??= new Command(obj =>
                {
                    using (var db = new dbContext())
                    {
                        EditWindow editWindow = new EditWindow();
                        editWindow.DataContext = this;
                        editWindow.Show();

                        HeroDB = db.Heroes.OrderBy(x => x.Name).Where(x => x.Name == DataGridItem.Name).FirstOrDefault();
                        HeroDB.Attribute = db.MainAttributes.Where(x => x.AttributeId == HeroDB.AttributeId).FirstOrDefault();
                        HeroDB.AttackType = attackType;
                        MainAttributeDB = HeroDB.Attribute.Name;

                        var RolesInDB = db.Roles.Where(x => x.Heroes.Any(y => y.HeroId == HeroDB.HeroId)).Select(x => x.Name).ToList();
                        foreach (var roleInDB in RolesInDB)
                        {
                            HeroDB.Roles.Add(db.Roles.Where(x => x.Name == roleInDB)
                                                        .FirstOrDefault());
                        }
                        RolesDB = new ObservableCollection<string>(HeroDB.Roles.Select(x => x.Name));
                    }
                });
            }
        }

        private Command addHeroRole;
        public Command AddHeroRole
        {
            get
            {
                return addHeroRole ??= new Command(obj =>
                {
                    if (RolesDB.Contains(SelectedRole))
                    {
                        MessageBox.Show("У героя уже есть такая роль!");
                    }
                    else if (SelectedRole == "")
                    {
                        MessageBox.Show("Сначала выберите роль!");
                    }
                    else
                    {
                        using (var db = new dbContext())
                        {
                            HeroDB.Roles.Add(db.Roles.Where(x => x.Name == SelectedRole).FirstOrDefault());
                            db.Entry(HeroDB).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            db.SaveChanges();
                            IsEnableHeroDetails = false;
                        }
                        RolesDB.Add(SelectedRole);
                    }
                });
            }
        }

        private Command deleteHeroRole;
        public Command DeleteHeroRole
        {
            get
            {
                return deleteHeroRole ??= new Command(obj =>
                {
                    if (SelectedRoleDB == null)
                    {
                        MessageBox.Show("Сначала выберите роль!");
                    }
                    else
                    {
                        using (var db = new dbContext())
                        {
                            var a = HeroDB.Roles.Where(x => x.Name == SelectedRoleDB).FirstOrDefault();
                            db.Attach(HeroDB);
                            HeroDB.Roles.Remove(a);
                            db.SaveChanges();
                            IsEnableHeroDetails = false;
                        }
                        RolesDB.Remove(SelectedRoleDB);
                    }
                });
            }
        }

        private Command editHero;
        public Command EditHero
        {
            get
            {
                return editHero ??= new Command(obj =>
                {
                    try
                    {
                        using (var db = new dbContext())
                        {
                            HeroDB.Attribute = db.MainAttributes.Where(x => x.Name == MainAttributeDB).FirstOrDefault();
                            db.Entry(HeroDB).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            db.SaveChanges();
                            IsEnableHeroDetails = false;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Некорректный ввод!");
                    }
                });
            }
        }

        private bool isEnableDeleteHero;
        public bool IsEnableDeleteHero
        {
            get
            {
                return isEnableHeroDetails;
            }
            set
            {
                isEnableHeroDetails = value;
                OnPropertyChanged();
            }
        }

        private Command deleteHero;
        public Command DeleteHero
        {
            get
            {
                return deleteHero ??= new Command(obj =>
                {
                    using (var db = new dbContext())
                    {
                        HeroDB = db.Heroes.OrderBy(x => x.Name).Where(x => x.Name == DataGridItem.Name).FirstOrDefault();
                        db.Entry(HeroDB).State = EntityState.Deleted;
                        HeroDB.Roles.Clear();
                        db.Heroes.Remove(HeroDB);
                        db.SaveChanges();
                        IsEnableDeleteHero = false;
                    }
                });
            }
        }

        private Command clearAllDB;
        public Command ClearAllDB
        {
            get
            {
                return clearAllDB ??= new Command(obj =>
                {
                    using (var db = new dbContext())
                    {
                        var AllHeroes = db.Heroes;
                        var AllRoles = db.Roles;
                        var AllAttributes = db.MainAttributes;
                        //db.Attach(AllHeroes);
                        foreach (Hero? hero in AllHeroes)
                        {
                            hero.Roles.Clear();
                        }
                        db.Heroes.RemoveRange(AllHeroes);
                        db.Roles.RemoveRange(AllRoles);
                        db.MainAttributes.RemoveRange(AllAttributes);
                        db.SaveChanges();
                        MessageBox.Show("База Данных очищена!");
                    }
                });
            }
        }

        private bool isEnableParsing;
        public bool IsEnableParsing
        {
            get
            {
                return isEnableParsing;
            }
            set
            {
                isEnableParsing = value; OnPropertyChanged();
            }
        }

        private Command startParsing;
        public Command StartParsing
        {
            get
            {
                return startParsing ??= new Command(async obj =>
                {
                    ProgressBarVisibility = "Visible";
                    IsEnableParsing = false;
                    await Parsing();
                    IsEnableParsing = true;
                    ProgressBarVisibility = "Hidden";
                });
            }
        }
        static async Task Parsing()
        {
            await Parser.GetInstance().Parse(@"https://dota2.fandom.com/wiki/Dota_2_Wiki");
        }
        private Command generateReport;
        public Command GenerateReport
        {
            get
            {
                return generateReport ??= new Command(obj =>
                {
                    ReportGenerator.GetInstance().GenerateReport(ShablonPath, SaveAsPath);
                });
            }
        }

        private string shablonPath;
        public string ShablonPath
        {
            get
            {
                return shablonPath;
            }
            set
            {
                shablonPath = value;
                OnPropertyChanged();
            }
        }

        private string saveAsPath;
        public string SaveAsPath
        {
            get
            {
                return saveAsPath;
            }
            set
            {
                saveAsPath = value;
                OnPropertyChanged();
            }
        }

        infoWindow info;

        private Command infoWindow;

        public Command InfoWindow
        {
            get
            {
                return infoWindow ??= new Command(obj =>
                {
                    info = new();
                    info.DataContext = this;
                    info.Show();
                });
            }
        }
        private Command applyPath;
        public Command ApplyPath
        {
            get
            {
                return applyPath ??= new Command(obj =>
                {
                    if (File.Exists(ShablonPath) && File.Exists(SaveAsPath)
                        && (Path.GetExtension(ShablonPath) == ".docx" || Path.GetExtension(ShablonPath) == ".doc")
                        && (Path.GetExtension(SaveAsPath) == ".docx" || Path.GetExtension(SaveAsPath) == ".doc"))
                    {
                        info.Close();
                    }
                    else
                    {
                        MessageBox.Show("Пути введены неверно! (Требуемый формат: doc или docx!)");
                    }
                });
            }
        }
    }
}
