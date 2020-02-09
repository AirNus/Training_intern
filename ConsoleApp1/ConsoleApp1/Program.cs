﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
namespace ConsoleApp1
{
    class TextWorker
    {
        public void ReadCSV(string[] rows)
        {
            string pathCSV = System.IO.Path.GetFullPath(@"data.CSV");
            using (StreamReader stReader = new StreamReader(pathCSV, Encoding.GetEncoding(1251)))
            {
                rows = stReader.ReadToEnd().Split(rows, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        public void WriteReport(string[] rows,int[,] InfoGoods,double TotalSumm,double Costs)
        {
            string pathReport = System.IO.Path.GetFullPath(@"Report.txt");
            using (StreamWriter stWriter = new StreamWriter(pathReport, false))
            {
                for(int counter = 1; counter<(rows.Length / 6); counter++)
                {
                    stWriter.WriteLine($"Товар{rows[counter * 6]}\nПродано {InfoGoods[counter-1,1]} штук\nЗакуплено {InfoGoods[counter - 1, 2]} штук");                   
                }
                stWriter.WriteLine($"Прибыль магазина  от продаж составила {Math.Round(TotalSumm - Costs,2)} руб");
                stWriter.WriteLine($"Затраченные средства на дозакупку товара {Costs} руб");
            }
        }
        public void WriteCSV(string[] rows)
        {
            string pathCSV = System.IO.Path.GetFullPath(@"data.CSV");
            using (StreamWriter sw = new StreamWriter(pathCSV, false, Encoding.GetEncoding(1251)))
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    sw.Write($"{rows[i]};");
                }
            }
        }

    }
    class Program
    {       
        public enum DaysWeek
        {
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        }
        static void Main(string[] args)
        {           
            double TotalSumm = 0, //Общая сумма заработанных деняк
                Costs = 0; // Расходы на дозакупку товара            
            string[] rows = { ";" };                        
            TextWorker tm = new TextWorker();
            tm.ReadCSV(rows);
            int[,] InfoGoods = new int[(rows.Length / 6) - 1, 3];   // двумерный массив который будет хранить доп.информацию о товарах(кол-во покупок\закупок)
            Days(ref InfoGoods,ref rows,ref TotalSumm,ref Costs);
            tm.WriteReport(rows, InfoGoods, TotalSumm, Costs);
            tm.WriteCSV(rows);
        }
        public static void Days(ref int[,] InfoGoods,ref string[] rows,ref double TotalSumm,ref double Costs)
        {
            NumberFormatInfo nfi = new CultureInfo("ru-RU", false).NumberFormat;
            nfi.NumberDecimalSeparator = ".";
            DaysWeek today = new DaysWeek();
            today = DaysWeek.Monday;
            byte days = 1;
            int hours = 8; // время открытия магазина в 8 часов
            while (days <= 30)
            {
                Console.Clear();
                while (hours < 22)
                {
                    TotalSumm += PurchaseInHour(hours, today, ref rows, ref InfoGoods, nfi);
                    hours++;
                }
                Procurement(ref rows, ref InfoGoods, ref Costs, nfi);
                hours = 8;
                today++;
                days++;
            }
        }
        public static void Procurement(ref string[] rows,ref int[,] InfoGoods,ref double Costs,NumberFormatInfo nfi)
        {
            for (int counter = 1; counter < (rows.Length / 6) - 1; counter++)
            {
                if (Convert.ToInt32(rows[counter * 6 + 5]) < 10)
                {
                    rows[counter * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[counter * 6 + 5]) + 150);
                    Costs += Convert.ToDouble(rows[counter * 6 + 1], nfi) * 150;
                    InfoGoods[counter - 1, 2] += 150;
                }
            }
        }
        public static double PurchaseInHour(int hours,DaysWeek today,ref string[] rows, ref int[,] InfoGoods,NumberFormatInfo nfi)
        {          
            Random rand = new Random();
            double TotalSumm = 0;           
            double price_goods; // цена на покупаемый товар с наценкой
            int count_people = rand.Next(1, 11); // количество людей которые зайдут в магазин в этот час
            int goods_purchased, // количество купленных товаров одним человеком
                goods, // указывает какой именно товар выбрал клиент
                quantity, // указывает какое количество определенного товара купил клиент
                goods_purchased_now;// показывает сколько  товаров куплено клиентом на данный момент
            int[] goods_bought = new int[(rows.Length / 6) - 1];
            while (count_people >= 0)
            {
                Array.Clear(goods_bought, 0, (rows.Length / 6) - 1);
                goods_purchased_now = 0;
                goods_purchased = rand.Next(0, 11);
                Console.WriteLine("Одним человеком...");
                while (goods_purchased_now != goods_purchased)
                {
                    goods = rand.Next(1, (rows.Length / 6));
                    if(goods_bought[goods - 1] == 1)
                    {
                        if(goods_bought.Average() == 1)
                        {
                            count_people--;
                            break;
                        }
                        continue;
                    }
                    quantity = rand.Next(1, goods_purchased - goods_purchased_now + 1);
                    if(Convert.ToInt32(rows[goods * 6 + 5]) < quantity)
                    {
                        quantity -= quantity - Convert.ToInt32(rows[goods * 6 + 5]);
                        if(quantity == 0)
                        {
                            goods_bought[goods - 1] = 1;
                            Console.WriteLine($"Клиент берет другой товар{rows[goods * 6]} - закончился");
                            continue;                         
                        }
                    }                   
                    if ((today == DaysWeek.Saturday || today == DaysWeek.Sunday) && quantity < 3)
                    {
                        price_goods = (Convert.ToDouble(rows[goods * 6 + 1], nfi) * 1.15);
                        TotalSumm += price_goods * quantity;
                        rows[goods * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[goods * 6 + 5]) - quantity);
                        Console.WriteLine($"Куплено {quantity} товаров {rows[goods * 6]} по цене {price_goods}  с наценкой 15%");
                    }
                    else if (hours > 18 && hours < 20)
                    {
                        price_goods = (Convert.ToDouble(rows[goods * 6 + 1], nfi) * 1.08);
                        TotalSumm += price_goods * quantity;
                        rows[goods * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[goods * 6 + 5]) - quantity);
                        Console.WriteLine($"Куплено {quantity} товаров {rows[goods * 6]} по цене {price_goods}  с наценкой 8%");
                    }
                    else if ((today != DaysWeek.Saturday && today != DaysWeek.Sunday) && quantity > 2)
                    {
                        price_goods = (Convert.ToDouble(rows[goods * 6 + 1], nfi) * 1.1);
                        TotalSumm += price_goods * 2;
                        rows[goods * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[goods * 6 + 5]) - 2);
                        Console.WriteLine($"Куплено 2 товара {rows[goods * 6]} по цене {price_goods}  с наценкой 10%");
                        price_goods = (Convert.ToDouble(rows[goods * 6 + 1], nfi) * 1.07);
                        TotalSumm += price_goods * (quantity - 2);
                        rows[goods * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[goods * 6 + 5]) - (quantity-2));
                        Console.WriteLine($"Так же куплено {quantity - 2} товаров {rows[goods * 6]} по цене {price_goods}  с наценкой 7%");
                    }
                    else if ((today == DaysWeek.Saturday || today == DaysWeek.Sunday) && quantity > 2)
                    {
                        price_goods = (Convert.ToDouble(rows[goods * 6 + 1], nfi) * 1.15);
                        TotalSumm += price_goods * 2;
                        rows[goods * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[goods * 6 + 5]) - 2);
                        Console.WriteLine($"Куплено 2 товара {rows[goods * 6]} по цене {price_goods}  с наценкой 15%");
                        price_goods = (Convert.ToDouble(rows[goods * 6 + 1], nfi) * 1.07);
                        TotalSumm += price_goods * (quantity - 2);
                        rows[goods * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[goods * 6 + 5]) - (quantity-2));
                        Console.WriteLine($"Так же куплено {quantity - 2} товаров {rows[goods * 6]} по цене {price_goods}  с наценкой 7%");
                    }
                    else
                    {
                        price_goods = (Convert.ToDouble(rows[goods * 6 + 1], nfi) * 1.1);
                        TotalSumm += price_goods * quantity;
                        rows[goods * 6 + 5] = Convert.ToString(Convert.ToInt32(rows[goods * 6 + 5]) - quantity);
                        Console.WriteLine($"Куплено {quantity} товаров {rows[goods * 6]} по цене {price_goods}  с наценкой 10%");
                    }
                    InfoGoods[goods - 1, 1] += quantity;
                    goods_purchased_now += quantity;
                    goods_bought[goods-1] = 1;
                }
                count_people--;              
            }
            return TotalSumm;             
        }
    }
}
