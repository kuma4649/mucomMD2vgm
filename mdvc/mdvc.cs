﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using System.Reflection;

namespace mdvc
{
    public class mdvc
    {
        /// <summary>
        /// コンパイル対象
        /// </summary>
        public string srcFn = "";
        public string desFn = "";

        public mdvc(string[] args)
        {

            //ファイル、オプションの指定無し
            if (args == null || args.Length < 1)
            {
                //disp usage
                Console.WriteLine(msg.get("I07000"));
                Environment.Exit(0);
            }

            int pos = 0;
            bool isLoopEx = false;
            int rendSecond = 600;

            //オプションを指定しているか
            if (args[0].IndexOf("-l") > -1)
            {
                pos++;
                isLoopEx = true;
                if (args[0].Length > 2)
                {
                    if (!int.TryParse(args[0].Substring(2), out rendSecond))
                    {
                        rendSecond = 0;
                    }
                    if (rendSecond == 0)
                    {
                        Console.WriteLine(msg.get("I07000"));
                        Environment.Exit(0);
                    }
                }
            }

            srcFn = args[pos];
            if (Path.GetExtension(srcFn) == "")
            {
                srcFn += ".muM";
            }

            if (args.Length > pos + 1)
            {
                desFn = args[pos + 1];
            }
            else
            {
                desFn = Path.Combine(Path.GetDirectoryName(srcFn), Path.GetFileNameWithoutExtension(srcFn) + ".vgm");
            }

            Core.Log.Debug = false;
            Core.Log.Open();
            Core.Log.Write("start compile thread");

            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = System.IO.Path.GetDirectoryName(myAssembly.Location);

            MucomMD2vgm mv = new MucomMD2vgm(srcFn, desFn, path, Disp, isLoopEx, rendSecond);
            int ret = mv.Start();

            if (ret == 0)
            {
                Console.WriteLine(msg.get("I0000"));
                Console.WriteLine(msg.get("I0001"));
                foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in mv.desVGM.chips)
                {
                    foreach (ClsChip chip in kvp.Value)
                    {
                        List<partWork> pw = chip.lstPartWork;
                        for (int i = 0; i < pw.Count; i++)
                        {
                            if (pw[i].clockCounter == 0) continue;

                            Console.WriteLine(string.Format(msg.get("I0002")
                                , pw[i].PartName //.Substring(0, 2).Replace(" ", "") + int.Parse(pw[i].PartName.Substring(2, 2)).ToString()
                                , pw[i].chip.Name.ToUpper()
                                , isLoopEx ? pw[i].loopInfo.totalCounter : pw[i].clockCounter
                                , isLoopEx ? pw[i].loopInfo.loopCounter.ToString() : "-"
                            ));
                        }
                    }
                }
            }

            Console.WriteLine(msg.get("I0003"));

            foreach (string mes in msgBox.getWrn())
            {
                Console.WriteLine(string.Format(msg.get("I0004"), mes));
            }

            foreach (string mes in msgBox.getErr())
            {
                Console.WriteLine(string.Format(msg.get("I0005"), mes));
            }

            Console.WriteLine("");
            Console.WriteLine(string.Format(msg.get("I0006"), msgBox.getErr().Length, msgBox.getWrn().Length));

            if (mv.desVGM != null)
            {
                if (mv.desVGM.loopSamples != -1)
                {
                    Console.WriteLine(string.Format(msg.get("I0007"), mv.desVGM.loopClock));
                    if (mv.desVGM.info.format == enmFormat.VGM)
                        Console.WriteLine(string.Format(msg.get("I0008")
                            , mv.desVGM.loopSamples
                            , mv.desVGM.loopSamples / 44100L));
                }

                Console.WriteLine(string.Format(msg.get("I0009"), mv.desVGM.lClock));
                if (mv.desVGM.info.format == enmFormat.VGM)
                    Console.WriteLine(string.Format(msg.get("I0010")
                        , mv.desVGM.dSample
                        , mv.desVGM.dSample / 44100L));

                if (mv.desVGM.ym2612[0].pcmDataEasy != null) Console.WriteLine(string.Format(msg.get("I0026"), mv.desVGM.ym2612[0].pcmDataEasy.Length));
            }

            Console.WriteLine(msg.get("I0050"));

            Core.Log.Write("end compile thread");
            Core.Log.Close();


            Environment.Exit(ret);
        }

        private void Disp(string msg)
        {
            Console.WriteLine(msg);
            Core.Log.Write(msg);
        }

    }
}
