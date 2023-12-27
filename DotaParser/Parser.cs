using AngleSharp.Dom;
using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotaParser.Models.Entities;
using DotaParser.Models;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Windows;

namespace DotaParser
{
    public class Parser
    {
        private static Parser? instance;
        public static Parser GetInstance()
        {
            instance ??= new Parser();
            return instance;
        }
        public async Task Parse(string url)
        {
            // Создаём контекст, который будет собирать код сайта
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var doc = await context.OpenAsync(url);

            //Заполнение таблицы основных атрибутов
            var attributesList = doc.QuerySelectorAll("div.mp-content tr th span a[href]").Select(elem => elem.GetAttribute("Title"));
            attributesList = attributesList.Distinct();
            MainAttribute mainAttribute;
            using (dbContext db = new dbContext())
            {
                foreach (string attribute in attributesList)
                {
                    var attributeInDB = db.MainAttributes.Where(x => x.Name == attribute).FirstOrDefault();
                    if (attributeInDB == null)
                    {
                        mainAttribute = new();
                        mainAttribute.AttributeId = Guid.NewGuid();
                        mainAttribute.Name = attribute;
                        db.Add(mainAttribute);
                    }
                }
                db.SaveChanges();
            }

            //Заполнение таблицы ролей
            var roleUrl = doc.QuerySelectorAll("#useful-articles li a[title=\"Roles\"]")
                .Select(elem => doc.Origin + elem.GetAttribute("href")).FirstOrDefault();
            doc = await context.OpenAsync(roleUrl);
            var listRoles = doc.QuerySelectorAll("#toc ul li.toclevel-1.tocsection-1 ul li a span.toctext")
                .Select(elem => elem.InnerHtml);
            Role? GameRole;
            using (dbContext db = new dbContext())
            {
                var rolesInDB = db.Roles.ToList();
                foreach (string role in listRoles)
                {
                    GameRole = rolesInDB.Where(x => x.Name == role).FirstOrDefault();
                    if (GameRole == null)
                    {
                        GameRole = new();
                        GameRole.RoleId = Guid.NewGuid();
                        GameRole.Name = role;
                        db.Add(GameRole);
                        rolesInDB.Add(GameRole);
                    }
                }
                db.SaveChanges();
            }

            //заполнение таблицы героев
            doc = await context.OpenAsync(url);
            var HeroUrls = doc.QuerySelectorAll("div.mp-content div.heroentry div a")
                .Select(elem => doc.Origin + elem.GetAttribute("href")).ToList();
            Hero? hero;
            string? heroName;
            string? heroMainAttribute;
            List<string> heroRoles;
            IElement? element;
            string? value;


            using (dbContext db = new dbContext())
            {
                var heroesInDB = db.Heroes.ToList();
                var mainAttributeInDB = db.MainAttributes.ToList();
                var rolesInDB = db.Roles.ToList();
                foreach (var HeroUrl in HeroUrls)
                {
                    int countAtts = 0;
                    doc = await context.OpenAsync(HeroUrl);
                    var infobox = doc.QuerySelector("table.infobox");
                    heroName = doc.QuerySelectorAll("div #firstHeading")
                                  ?.Select(elem => elem.InnerHtml)
                                  ?.FirstOrDefault()
                                  ?.Trim();
                    if (heroName == null) continue;

                    hero = heroesInDB.Where(x => x.Name == heroName).FirstOrDefault();
                    if (hero == null)
                    {
                        hero = new();

                        //id
                        hero.HeroId = Guid.NewGuid();

                        //name
                        hero.Name = heroName;

                        //main attribute
                        var elements = doc.QuerySelectorAll("#primaryAttribute a")
                                               ?.Select(elem => elem.GetAttribute("title"));
                        foreach (var e in elements)
                        {
                            countAtts++;
                        }
                        if (countAtts > 1) heroMainAttribute = "Universal";
                        else
                            heroMainAttribute = doc.QuerySelectorAll("#primaryAttribute a")
                                                   ?.Select(elem => elem.GetAttribute("title"))
                                                   ?.FirstOrDefault();
                        if (heroMainAttribute == null) continue;
                        hero.Attribute = mainAttributeInDB
                                           .Where(x => x.Name == heroMainAttribute)
                                           .FirstOrDefault();
                        if (hero.Attribute == null) continue;

                        //health
                        element = doc.QuerySelector("table.infobox a[title=\"Health\"]")
                                        ?.GetAncestor<IElement>()
                                        ?.GetAncestor<IElement>();
                        if (element == null) continue;
                        else
                        {
                            value = element.QuerySelector("td:nth-child(3)")
                                           ?.InnerHtml
                                           .Trim();
                            if (value == null) continue;
                            else
                                hero.Health = int.Parse(value);
                        }

                        //mana
                        element = doc.QuerySelector("table.infobox a[title=\"Mana\"]")
                                     ?.GetAncestor<IElement>()
                                     ?.GetAncestor<IElement>();
                        if (element == null) continue;
                        else
                        {
                            value = element.QuerySelector("td:nth-child(3)")
                                          ?.InnerHtml.Trim();
                            if (value == null) continue;
                            else
                                hero.Mana = int.Parse(value);
                        }

                        //armor
                        element = doc.QuerySelector("table.infobox a[title=\"Armor\"]")
                                     ?.GetAncestor<IElement>()
                                     ?.GetAncestor<IElement>();
                        if (element == null) continue;
                        else
                        {
                            value = element.QuerySelector("td:nth-child(3) span")
                                           ?.InnerHtml.Trim().Replace(".", ",");
                            if (value == null) continue;
                            else
                                hero.Armor = double.Parse(value);
                        }

                        //magic resistance
                        element = doc.QuerySelector("table.infobox a[title=\"Magic Resistance\"]")
                                                   ?.GetAncestor<IElement>()
                                                   ?.GetAncestor<IElement>();
                        if (element == null) continue;
                        else
                        {
                            value = element.QuerySelector("td:nth-child(3)")
                                           ?.InnerHtml.Trim().Replace("%", "").Replace(".", ",");
                            if (value == null) continue;
                            else
                                hero.MagicResistance = double.Parse(value);
                        }

                        //damage
                        var averageDamage = doc.QuerySelector("table.infobox a[title=\"Attack Damage\"]")
                                                   ?.GetAncestor<IElement>()
                                                   ?.GetAncestor<IElement>()
                                                   ?.QuerySelector("td:nth-child(3) p")
                                                   ?.InnerHtml.Trim().Replace("\"\"", "").Split("<br>") //INNERTEXT!!!!!!!!!!!!!!!!!!
                                                   .ToList();
                        if (averageDamage == null) continue;
                        hero.Damage = (int.Parse(averageDamage[0]) + int.Parse(averageDamage[1])) / 2;

                        //movespeed
                        element = doc.QuerySelector("table.infobox a[title=\"Movement speed\"]")
                                                   ?.GetAncestor<IElement>()
                                                   ?.GetAncestor<IElement>();
                        if (element == null) continue;
                        else
                        {
                            value = element.QuerySelector("td:nth-child(3)")
                                           ?.InnerHtml.Trim();
                            if (value == null) continue;
                            else
                                hero.MoveSpeed = int.Parse(value);
                        }

                        //attack type
                        var heroAttackType = doc.QuerySelector("table.infobox a[title=\"Attack Range\"]")
                                                   ?.GetAncestor<IElement>()
                                                   ?.GetAncestor<IElement>()
                                                   ?.QuerySelector("span a")
                                                   ?.GetAttribute("title");
                        if (heroAttackType == null) continue;
                        hero.AttackType = heroAttackType == "Melee" ? false : true;

                        //roles
                        heroRoles = doc.QuerySelectorAll("a[title=\"Role\"]")
                                       .Select(elem => elem.InnerHtml)
                                       .ToList();
                        foreach (string role in heroRoles)
                        {
                            hero.Roles.Add(rolesInDB.Where(x => x.Name == role)
                                                    .FirstOrDefault());
                        }

                        db.Heroes.Add(hero);
                        heroesInDB.Add(hero);
                        //db.SaveChanges();
                    }
                    db.SaveChanges();
                }
            }
            MessageBox.Show("Парсинг завершён успешно!");
        }
    }
}
