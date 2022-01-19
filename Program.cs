using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using AwokeKnowing.GnuplotCSharp;

namespace Ti
{
    class Program
    {
        public static class MyGlobals
        {
            public static string GlobalPath = "C:/Program Files/LAMMPS/bin";
        } // Глобальный путь к папки LAMMPS/bin
        public static void AtomskCalc()
        {
            Process psi = new Process();
            psi.StartInfo.Verb = "runas";
            psi.StartInfo.FileName = "cmd";
            psi.StartInfo.Arguments = @"/c cd " + MyGlobals.GlobalPath + "/Ti/Simpl && atomsk --create hcp 2.9465 4.7867 Ti orient [01-10] [0001] [2-1-10]  -orthogonal-cell Ti.xsf +" + //orient [0-110] [0001] [2-1-10]
                    " && atomsk --polycrystal Ti.xsf ../../polyX.txt Ti_GB_HCP.cfg && atomsk Ti_GB_HCP.cfg lammps";
            // psi.StartInfo.Arguments = @"/c cd " + MyGlobals.GlobalPath + "/TI/Test_MinimumEGB && atomsk --create hcp 2.9465 4.7867 Ti orient [0-110] [0001] [2-1-10] -orthogonal-cell Ti.xsf +" + //orient [0-110] [0001] [2-1-10]
            //        " && atomsk --polycrystal Ti.xsf ../../polyX.txt Ti_GB_HCP.cfg && atomsk Ti_GB_HCP.cfg lammps";
            psi.Start();

            psi.WaitForExit();
        }
        public static void CalcAtomsk(string angle, double x, double y)
        {
            Process psi = new Process();
            psi.StartInfo.CreateNoWindow = true;
            psi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            psi.StartInfo.Verb = "runas";
            psi.StartInfo.FileName = "cmd";
            psi.StartInfo.Arguments = @"/c cd " + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/Atomsk_xyz/" + angle + "/" + x + "_" + y + " && atomsk --create hcp 2.9465 4.7867 Ti orient [0-110] [0001] [2-1-10] -orthogonal-cell Ti" + angle + "" + x + "" + y + ".xsf +" + //orient [0-110] [0001] [2-1-10]
                " && atomsk --polycrystal Ti" + angle + "" + x + "" + y + ".xsf ../../../../../polyX" + angle + "" + x + "" + y + ".txt Ti_GB_HCP" + angle + "" + x + "" + y + ".cfg && atomsk Ti_GB_HCP" + angle + "" + x + "" + y + ".cfg lammps";
            psi.Start();
            psi.WaitForExit();
        }
        public static void CalcLammps(string W)
        {
            Process psi = new Process();
            psi.StartInfo.Verb = "runas";
            psi.StartInfo.FileName = "cmd";
            psi.StartInfo.Arguments = @"/c cd " + MyGlobals.GlobalPath + " && mpiexec -np 4 -localroot lmp -in in." + W;
            psi.Start();
            psi.WaitForExit();
        }
        public static void CalcLammpsHIDE(string W)
        {
            Process psi = new Process();
            psi.StartInfo.CreateNoWindow = true;
            psi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            psi.StartInfo.Verb = "runas";
            psi.StartInfo.FileName = "cmd";
            psi.StartInfo.Arguments = @"/c cd " + MyGlobals.GlobalPath + " && mpiexec -np 1 -localroot lmp -in in." + W;
            psi.Start();
            psi.WaitForExit();
        }
        public static void CalcLammps3(string S, double xl, double yl)
        {
            Process psi = new Process();
            psi.StartInfo.CreateNoWindow = true;
            psi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            psi.StartInfo.Verb = "runas";
            psi.StartInfo.FileName = "cmd";
            psi.StartInfo.Arguments = @"/c cd " + MyGlobals.GlobalPath + " && mpiexec -np 4 -localroot lmp -in in." + S + "" + xl + "" + yl;
            psi.Start();
            psi.WaitForExit();
        }
        public static void DelDir(string AngleDel)
        {
            DirectoryInfo dirInfoD = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + AngleDel);

            foreach (FileInfo file in dirInfoD.GetFiles())
            {
                file.Delete();
            }
        }
        public static void DelTI()
        {
            DirectoryInfo dirInfoD = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Simpl");

            foreach (FileInfo file in dirInfoD.GetFiles())
            {
                file.Delete();
            }
        }
        public static void CreatLMP_EGB(string ANG, double cx, double cy)
        {
            //string pthlmpfile = @"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + ANG;
            using (StreamWriter xyzdata = File.AppendText(@"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + ANG + "" + cx + "" + cy + ""))
            {
                xyzdata.Write(
                    "\nvariable lattype string  \"hcp\"" +
                    "\nvariable latparam equal  2.92320957630403" +                 //2.922981209872 //2.9465      /\    2.92321     /\ 3.18126047522533
                    "\nvariable potfile string  'Ti3.eam.fs'" +            //Ti1.eam.fs  /\    Ti3.eam.fs  /\ Mg_mm.eam.fs
                    "\nvariable METALL string   \"Ti\"" +
                    "\nvariable DATA_DIR string \"" + ANG + "\"" +
                    "\n# ---------- Initialize Simulation --------------------- " +
                    "\nclear" +
                    "\nunits metal" +
                    "\ndimension 3" +
                    "\nboundary p p p" +
                    "\natom_style atomic" +
                    "\n# ---------- Create Atomistic Structure --------------------- " +
                    "\nshell cd TI/Mpi_Ti/Atomsk_xyz/" + ANG + "/" + cx + "_" + cy + " " +
                    "\nread_data Ti_GB_HCP" + ANG + "" + cx + "" + cy + ".lmp" +
                    "\nshell cd ../../../../../ " +
                    "\npair_style eam/fs " +
                    "\npair_coeff * * ${potfile} ${METALL}" +
                    "\nneighbor  2.0 bin" +
                    "\nneigh_modify delay 10 check yes" +
                    "\n#include Displace_atom_overlap.txt" +
                    "\ndisplace_atoms all move 0 0 0 units lattice " +
                    "\ndelete_atoms overlap 1.7 all all" +
                    "\n# ====================================== Define Settings ================================================# " +
                    "\ncompute csym all centro/atom 12" +
                    "\ncompute eng all pe/atom" +
                    "\ncompute eatoms all reduce sum c_eng" +
                    "\nreset_timestep 0" +
                    "\n#thermo 100 " +
                    "\n# -------------------------------------------- DATA output ----------------------------------------------#" +
                    "\n#thermo_style custom step pe lx ly lz pxx pyy pzz c_eatoms " +
                    "\nvariable X equal \"lx\"" +
                    "\nvariable Y equal \"ly\"" +
                    "\nshell mkdir Dump_Ti/${DATA_DIR}" +
                    "\nshell mkdir Dump_Ti/${DATA_DIR}/Y_${Y}" +
                    "\nshell cd Dump_Ti/${DATA_DIR}/Y_${Y}" +
                    "\n# ================================== variable ===========================================#" +
                    "\n#dump 	1 all custom 20000 dump.Ti.minimization id mass type xs ys zs c_eng fx fy fz " +
                    "\n# ====================================== Run Minimization ===============================================# " +
                    "\n#min_style cg " +
                    "\n#minimize 1e-25 1e-25 0 0 " +
                    "\n# ------------Now allow the box to expand/contract perpendicular to the grain boundary-------------------#" +
                    "\nreset_timestep 0" +
                    "\nthermo 100" +
                    "\n# -------------------------------------------- DATA output ----------------------------------------------#" +
                    "\nthermo_style custom step pe lx ly lz press pxx pyy pzz c_eatoms" +
                    "\ndump 	1 all custom 50 dump_A_" + ANG + "_X_${X}_Y_${Y}_.minimization id mass type xs ys zs c_csym c_eng fx fy fz" +
                    "\n#fix 1 all box/relax y 0 vmax 0.001" +
                    "\n#====================================== Run Minimization 2 =============================================" +
                    "\nmin_style cg" +
                    "\nminimize 1e-25 1e-25 5000 10000" +
                    "\n#unfix 1" +
                    "\nundump 1" +
                    "\nshell cd ../../../" +
                    "\n# ====================================== Calculate GB Energy ============================================# " +
                    "\nshell cd TI/Mpi_Ti/Atomsk_xyz/" + ANG + "" +
                    "\nvariable minimumenergy equal -5.39921013389605" +                               //-5.24432488658175  //-5.34386193959105 // -5.39921013389605 // Mg -1.5286625708582
                    "\nvariable esum equal \"v_minimumenergy * count(all)\"" +
                    "\nvariable xseng equal \"c_eatoms - (v_minimumenergy * count(all))\"" +
                    "\nvariable gbarea equal \"lx * lz * 2\"" +
                    "\nvariable gbe equal \"(c_eatoms - (v_minimumenergy * count(all)))/v_gbarea\"" +
                    "\nvariable gbemJm2 equal ${gbe}*16021.7733" +
                    "\n#shell rm dump.${X}.minimization" +
                    "\n#Print \"N atoms is ${No}\"" +
                    "\nprint \"All done\"" +
                    "\nlog log.gbe_X_" + cx + "_Y" + cy + "" +
                    "\nprint \"Xsize = ${X}\"" +
                    "\nprint \"Ysize = ${Y}\"" +
                    "\nprint \"GrainBoundaryEnergy = ${gbemJm2} mJ/m^2\"");
                xyzdata.Close();
            }
        }
        public static void Creat_VFE_LMP(string SGMVFE, string cx, string cy, string cz)
        {
            //string pthlmpfile = @"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + ANG;
            using (StreamWriter xyzdata = File.AppendText(@"" + MyGlobals.GlobalPath + "/in.VFE_" + SGMVFE + "_Ti_" + cx + "_" + cy + "_" + cz + ""))
            {
                xyzdata.Write(
                    "\nvariable Angleses string '" + SGMVFE + "'" +
                    "\nvariable pos string  \"" + cx + " " + cy + " " + cz + "\"" +
                    "\nvariable lattype string  \"hcp\"" +
                    "\nvariable latparam equal  2.92320957630403" +                 //2.922981209872 //2.9465      /\    2.92321     
                    "\nvariable potstyle string  'eam/fs'" +
                    "\nvariable potfile string  'Ti3.eam.fs'" +            //Ti1.eam.fs  /\    Ti3.eam.fs 
                    "\nvariable METALL string   \"Ti\"" +
                    "\n# ---------- Initialize Simulation --------------------- " +
                    "\nclear" +
                    "\nunits metal" +
                    "\ndimension 3" +
                    "\nboundary p p p" +
                    "\natom_style atomic" +
                    "\n# ---------- Create Atomistic Structure --------------------- " +
                    "\nshell cd TI_STR/" +
                    "\nread_data HCPGB_" + SGMVFE + ".lmp" +
                    "\nshell cd ../ " +
                    "\n# ============================ Define Interatomic Potential ========================================#" +
                    "\npair_style ${potstyle} " +
                    "\npair_coeff * * ${potfile} ${METALL}" +
                    "\n#displace_atoms all move 0 0 0 units lattice " +
                    "\n#delete_atoms overlap 1.5 all all" +
                    "\n# ====================================== Define Settings ================================================# " +
                    "\ncompute csym all centro/atom 12" +
                    "\ncompute eng all pe/atom" +
                    "\ncompute eatoms all reduce sum c_eng" +
                    "\nreset_timestep 0" +
                    "\nthermo 100 " +
                    "\n# -------------------------------------------- DATA output ----------------------------------------------#" +
                    "\nthermo_style custom step pe lx ly lz pxx pyy pzz c_eatoms " +
                    "\nvariable X equal \"lx\"" +
                    "\nvariable Y equal \"ly\"" +
                    "\nshell mkdir VFE/${METALL}" +
                    "\nshell mkdir VFE/${METALL}/" + SGMVFE + "" +
                    "\n#shell cd    VFE/${METALL}/" + SGMVFE + "" +
                    "\n# ================================== variable ===========================================#" +
                    "\n#dump 1 all custom 400 dump.${METALL}_relax_${Sigma}.1.* id type xs ys zs c_csym c_eng " +
                    "\n# ====================================== Run Minimization ===============================================# " +
                    "\n#fix 1 all box/relax y 0 vmax 0.001" +
                    "\nmin_style cg " +
                    "\nminimize 1e-25 1e-25 5000 10000 " +
                    "\nrun 0" +
                    "\n#undump 1" +
                    "\nthermo 100" +
                    "\nvariable E equal \"c_eatoms\"" +
                    "\nvariable Eo equal $E" +
                    "\nvariable N equal count(all)" +
                    "\nvariable No equal $N" +
                    "\n#================================ Create_of_VACANCY ================================================#" +
                    "\nif \"${lattype} == bcc\" then \"variable r2 equal sqrt(${latparam}^2+${latparam}^2+${latparam}^2)/4\" &" +
                    "\nelse \"variable r2 equal sqrt(${latparam}^2+${latparam}^2)/4\"" +
                    "\nregion select sphere ${pos} ${r2} units box" +
                    "\ndelete_atoms region select compress yes" +
                    "\nreset_timestep  0" +
                    "\nthermo 100" +
                    "\nthermo_style custom step pe lx ly lz press pxx pyy pzz c_eatoms" +
                    "\ninclude dumpset" + SGMVFE + "_" + cx + "_" + cy + "_" + cz + "" +
                    "\nmin_style cg" +
                    "\nminimize 1e-25 1e-25 5000 10000" +
                    "\nvariable Ef equal \"c_eatoms\"" +
                    "\nvariable Ev equal (${Ef}-((${No}-1)/${No})*${Eo})" +
                    "\n# ====================================== SIMULATION DONE ============================================# " +
                    "\nshell cd ../../../" +
                    "\nprint \"All done\"" +
                    "\nlog log.VFE_" + SGMVFE + "_X_" + cx + "_Y_" + cy + "_Z_" + cz + "" +
                    "\nprint \"Total number of atoms = ${No}\"" +
                    "\nprint \"Initial energy of atoms = ${Eo}\"" +
                    "\nprint \"Final energy of atoms = ${Ef}\"" +
                    "\nprint \"Xsize = ${X}\"" +
                    "\nprint \"Ysize = ${Y}\"" +
                    "\nprint \"Vacancy formation energy = ${Ev}\"");
                xyzdata.Close();
            }
        }
        public static void CreatIFELMP(string SGM, double cx, double cy)
        {
            //string pthlmpfile = @"" + MyGlobals.GlobalPathMpi + "/in.Ti_HCP_" + ANG;
            using (StreamWriter xyzdata = File.AppendText(@"" + MyGlobals.GlobalPath + "/in.IFE_Ti_" + cx + "" + cy + ""))
            {
                xyzdata.Write(
                    "\nvariable Sigma string '" + SGM + "'" +
                    "\nvariable pos string  \"" + cx + " " + cy + " 23.9 \"" +
                     "\nvariable lattype string  \"hcp\"" +
                    "\nvariable latparam equal  2.92320957630403" +                 //2.922981209872 //2.9465      /\    2.92321     
                    "\nvariable potstyle string  'eam/fs'" +
                    "\nvariable potfile string  'Ti3.eam.fs'" +            //Ti1.eam.fs  /\    Ti3.eam.fs 
                    "\nvariable METALL string   \"Ti\"" +
                    "\n# ---------- Initialize Simulation --------------------- " +
                    "\nclear" +
                    "\nunits metal" +
                    "\ndimension 3" +
                    "\nboundary p p p" +
                    "\natom_style atomic" +
                    "\n# ---------- Create Atomistic Structure --------------------- " +
                    "\nshell cd TI_STR/" +
                    "\nread_data HCPGB_" + SGM + ".lmp" +
                    "\nshell cd ../ " +
                    "\n# ============================ Define Interatomic Potential ========================================#" +
                    "\npair_style ${potstyle} " +
                    "\npair_coeff * * ${potfile} ${METALL}" +
                    "\n#displace_atoms all move 0 0 0 units lattice " +
                    "\n#delete_atoms overlap 1.8 all all" +
                    "\n# ====================================== Define Settings ================================================# " +
                    "\ncompute csym all centro/atom 12" +
                    "\ncompute eng all pe/atom" +
                    "\ncompute eatoms all reduce sum c_eng" +
                    "\nreset_timestep 0" +
                    "\nthermo 10 " +
                    "\n# -------------------------------------------- DATA output ----------------------------------------------#" +
                    "\nthermo_style custom step pe lx ly lz pxx pyy pzz c_eatoms " +
                    "\nvariable X equal \"lx\"" +
                    "\nvariable Y equal \"ly\"" +
                    "\nshell mkdir IFE/${METALL}" +
                    "\nshell mkdir IFE/${METALL}/" + SGM + "" +
                    "\nshell cd    IFE/${METALL}/" + SGM + "" +
                    "\n# ================================== variable ===========================================#" +
                    "\n#dump 1 all custom 400 dump.${METALL}_relax_${Sigma}.1.* id type xs ys zs c_csym c_eng " +
                    "\n# ====================================== Run Minimization ===============================================# " +
                    "\nmin_style cg " +
                    "\nminimize 1e-25 1e-25 5000 5000 " +
                    "\nrun 0" +
                    "\n#undump 1" +
                    "\nthermo 100" +
                    "\nvariable E equal \"c_eatoms\"" +
                    "\nvariable Eo equal $E" +
                    "\nvariable N equal count(all)" +
                    "\nvariable No equal $N" +
                    "\n#================================ Create_of_VACANCY ================================================#" +
                    "\ncreate_atoms 1 single ${pos} units box" +
                    "\n#delete_atoms overlap 1.9 all all" +
                    "\nreset_timestep  0" +
                    "\nthermo 10" +
                    "\nthermo_style custom step pe lx ly lz press pxx pyy pzz c_eatoms" +
                    "\ndump 1 all custom 20000 dump.IFEdef_X_" + cx + "_Y_" + cy + " id type xs ys zs c_csym c_eng " +
                    "\nmin_style cg" +
                    "\nminimize 1e-25 1e-25 5000 5000" +
                    "\nvariable Ef equal \"c_eatoms\"" +
                    "\nvariable Ei equal (${Ef}-((${No}+1)/${No})*${Eo})" +
                    "\n# ====================================== SIMULATION DONE ============================================# " +
                    "\nshell cd ../../../" +
                    "\nprint \"All done\"" +
                    "\nlog log.IFE_X_" + cx + "_Y" + cy + "" +
                    "\nprint \"Total number of atoms = ${No}\"" +
                    "\nprint \"Initial energy of atoms = ${Eo}\"" +
                    "\nprint \"Final energy of atoms = ${Ef}\"" +
                    "\nprint \"Xsize = ${X}\"" +
                    "\nprint \"Ysize = ${Y}\"" +
                    "\nprint \"Interstitial formation energy = ${Ei}\"");
                xyzdata.Close();
            }
        }
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            for (; ; )
            {
                string[] SGMA = { };
                Console.WriteLine("Выберите варианты дейсвтия:" +
                    "\n 1.ATOMSK!!!!; " +
                    "\n 2.!!!!; " +
                    "\n 3.MPI_FIND_STR(Output_3d_heatMap); " +
                    "\n 4.Последовательный расчет Ti" +
                    "\n 5.Parallel_calc_all_angle;" +
                    "\n 6.VFE[];" +
                    "\n 7.IFE[]");
                string r = Console.ReadLine();
                switch (r)
                {
                    case "1":
                        DelTI();
                        AtomskCalc();
                        break;
                    case "2":
                        DelTI();
                        DirectoryInfo dirInf = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/Dump_Ti_min");

                        foreach (FileInfo file in dirInf.GetFiles())
                        {
                            file.Delete();
                        }
                        AtomskCalc();
                        CalcLammps("Ti_Dump");
                        DirectoryInfo DeleteMinw = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/Ti/Test_MinimumEGB/");

                        foreach (FileInfo file in DeleteMinw.GetFiles())
                        {
                            file.Delete();
                        }
                        //  CreatLMP_EGB("20");
                        break;
                    case "3":
                        Console.WriteLine("Введите Угол/2 : ");
                        string Angle = Console.ReadLine();
                        Console.WriteLine("Введите начальное значение Х : ");
                        string Xbegin = Console.ReadLine();
                        Console.WriteLine("Введите конечное значение  Х : ");
                        string Xending = Console.ReadLine();
                        Console.WriteLine("Введите начальное значение Y : ");
                        string Ybegin = Console.ReadLine();
                        Console.WriteLine("Введите конечное значение  Y : ");
                        string Yending = Console.ReadLine();
                        Console.WriteLine("Введите шаг : ");
                        string Step_Ti = "1";
                        //string Step_Ti = Console.ReadLine();
                        double MinEGB = 12000; string Xdir = ""; string Ydir = "";
                        //double Step_Ti = Math.Abs(Math.Cos((Double.Parse(Angle)*Math.PI)/180)) * 2.9465 * Math.Sqrt(3)*0.5;
                        //double Step_TiY =Math.Abs(Math.Sin((Double.Parse(Angle)*Math.PI)/180) * 4.7867);

                        //string[] countangle = { "4.7", "6.6", "8.2", "10.9", "13.9", "15", "16.15", "21.05", "22.4" };

                        //for (int a = 0; a < countangle.Length; a++)
                        //{
                        //string Angle = countangle[a];

                        DirectoryInfo NewDir = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle);
                        if (!NewDir.Exists)
                        {
                            NewDir.Create();
                        }
                        DirectoryInfo NewDir22 = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle + "/" + Xbegin + "_" + Ybegin);
                        if (!NewDir22.Exists)
                        {
                            NewDir22.Create();
                        }
                        string TEXTMAss = "Gbe SizeX SizeY  ";
                        
                        //Console.WriteLine("Шаг по Х : " + Step_Ti);
                        //for (double xc = double.Parse(Xbegin); xc <= double.Parse(Xending); xc += Step_Ti)
                        Parallel.For(int.Parse(Xbegin), int.Parse(Xending), xc =>
                        {
                           
                            //Console.WriteLine("Осталось : " + (((xc- double.Parse(Xbegin)) /(double.Parse(Xending) - double.Parse(Xbegin)))*100) + " %");
                            DirectoryInfo NewDir2 = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle + "/" + xc + "_" + Ybegin);
                            if (!NewDir2.Exists)
                            {
                                NewDir2.Create();
                            }
                            using (StreamWriter xyzdata = File.AppendText(@"" + MyGlobals.GlobalPath + "/polyX" + Angle + "" + xc + "" + double.Parse(Ybegin) + ".txt"))
                            {
                                xyzdata.Write("box " + xc + " " + double.Parse(Ybegin) + " 9.5734\n" +
                                            "node 0.5*box 0.25*box 0.5*box 0° 0° -" + Angle + "°\n" +
                                            "node 0.5*box 0.75*box 0.5*box 0° 0° " + Angle + "°"); ;
                            }
                            CalcAtomsk(Angle, xc, double.Parse(Ybegin));
                            CreatLMP_EGB(Angle, xc, double.Parse(Ybegin));
                            CalcLammps3("Ti_HCP_" + Angle, xc, double.Parse(Ybegin));
                            string ReadLogLammps = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle + "/log.gbe_X_" + xc + "_Y" + Ybegin);
                            string Size_Y = ReadLogLammps.Substring(ReadLogLammps.IndexOf("Ysize =") + 23, 2);
                            string Size_X = ReadLogLammps.Substring(ReadLogLammps.IndexOf("Xsize =") + 23, 2);
                            //Console.WriteLine(Size_X);
                            //Console.WriteLine(Size_Y);
                            string ReadLogGbe = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle + "/log.gbe_X_" + xc + "_Y" + Ybegin);
                            string GBE = ReadLogGbe.Substring(ReadLogGbe.IndexOf("GrainBoundaryEnergy =") + 64, 10);

                            TEXTMAss = TEXTMAss.Insert(16, "\n" + GBE + " " + xc + " " + Ybegin);
                            //////////////////////////////////////ENDING////////////////////////////////////////
                            //DelDir(Angle,xc, int.Parse(Ybegin));
                            if (MinEGB > Convert.ToDouble(GBE))
                            {
                                MinEGB = Convert.ToDouble(GBE); Xdir = Convert.ToString(xc); Ydir = Ybegin;
                            }
                            File.Delete(@"" + MyGlobals.GlobalPath + "/polyX" + Angle + "" + xc + "" + double.Parse(Ybegin) + ".txt");
                            File.Delete(@"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + Angle + "" + xc + "" + double.Parse(Ybegin));


                            Console.WriteLine("Данные по энергии: Позиция X = " + xc + " Позиция Y = " + Ybegin + " Egb = " + GBE + " MinEgm = " + MinEGB);

                            //for (double yc = double.Parse(Ybegin) + double.Parse(Step_Ti); yc <= double.Parse(Yending); yc += double.Parse(Step_Ti))
                            Parallel.For((int)(double.Parse(Ybegin) + double.Parse(Step_Ti)), (int)double.Parse(Yending), yc =>
                            {
                                
                                DirectoryInfo NewDir3 = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle + "/" + xc + "_" + yc);
                                if (!NewDir3.Exists)
                                {
                                    NewDir3.Create();
                                }
                                using (StreamWriter xyzdata = File.AppendText(@"" + MyGlobals.GlobalPath + "/polyX" + Angle + "" + xc + "" + yc + ".txt"))
                                {
                                    xyzdata.Write("box " + xc + " " + yc + " 9.5734\n" +
                                                      "node 0.5*box 0.25*box 0.5*box 0° 0° -" + Angle + "°\n" +
                                                      "node 0.5*box 0.75*box 0.5*box 0° 0° " + Angle + "°");
                                }
                                CalcAtomsk(Angle, xc, yc);
                                CreatLMP_EGB(Angle, xc, yc);
                                CalcLammps3("Ti_HCP_" + Angle, xc, yc);
                                string ReadLogLammpsY = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle + "/log.gbe_X_" + xc + "_Y" + yc);
                                string Size_YY = ReadLogLammpsY.Substring(ReadLogLammpsY.IndexOf("Ysize =") + 23, 2);
                                string Size_XY = ReadLogLammpsY.Substring(ReadLogLammpsY.IndexOf("Xsize =") + 23, 2);
                                //Console.WriteLine(Size_XY);
                                //Console.WriteLine(Size_YY);
                                string ReadLogGbeY = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Angle + "/log.gbe_X_" + xc + "_Y" + yc);
                                string GBEY = ReadLogGbeY.Substring(ReadLogGbeY.IndexOf("GrainBoundaryEnergy =") + 64, 10);
                                TEXTMAss = TEXTMAss.Insert(16, "\n" + GBEY + " " + xc + " " + yc);
                                if (MinEGB > Convert.ToDouble(GBEY))
                                {
                                    MinEGB = Convert.ToDouble(GBEY); Xdir = Convert.ToString(xc); Ydir = Convert.ToString(yc);
                                }
                                File.Delete(@"" + MyGlobals.GlobalPath + "/polyX" + Angle + "" + xc + "" + yc + ".txt");
                                File.Delete(@"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + Angle + "" + xc + "" + yc);
                                Console.WriteLine("Данные по энергии: Позиция X = " + xc + " Позиция Y = " + yc + " Egb = " + GBEY + " MinEgm = " + MinEGB);
                            });
                            //Console.WriteLine("Осталось : " + ((1 - ((double.Parse(Xending) - xc) / (double.Parse(Xending) - xc))) * 100));
                        });
                        Console.WriteLine("Минимальное значение ЭНЕРГИИ GB : " + MinEGB); Console.WriteLine("Ширина блока = " + Xdir); Console.WriteLine("Высота блока = " + Ydir);

                        File.WriteAllText(@"" + MyGlobals.GlobalPath + "/polyX.txt", string.Empty);
                        using (StreamWriter olltxt = File.AppendText(@"" + MyGlobals.GlobalPath + "//polyX.txt"))
                        {
                            olltxt.Write("box " + Xdir + " " + Ydir + " 9.5734\n" +
                                "node 0.5*box 0.25*box 0.5*box 0° 0° -" + Angle + "°\n" +
                                "node 0.5*box 0.75*box 0.5*box 0° 0° " + Angle + "°");
                        }
                        //CalcCMD();
                        CalcLammps("Ti_Dump");

                        TEXTMAss = TEXTMAss.Replace("Gbe SizeX SizeY ", "");
                        File.WriteAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/DATA/Data_" + Angle + ".txt", TEXTMAss);
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///////////////////////////////////////////////////GNUPLOT/////////////////////////////////////////////////////////
                        GnuPlot.WriteLine("reset");
                        GnuPlot.Set("view map");
                        GnuPlot.Set("pm3d at b map");
                        GnuPlot.Set("dgrid3d 500,500,2");
                        GnuPlot.Set("xlabel \"Upscale to X(A)\"");
                        GnuPlot.Set("ylabel \"Upscale to Y(A)\"");
                        GnuPlot.Set("term png");
                        GnuPlot.Set("output \"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/DATA/Plot_3D_A_" + Angle + ".png\"");
                        GnuPlot.SPlot(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/DATA/Data_" + Angle + ".txt", "u 3:2:1 with image");
                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        DelDir(Angle);
                        DirectoryInfo DeleteMin = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Test_MinimumEGB/");

                        foreach (FileInfo file in DeleteMin.GetFiles())
                        {
                            file.Delete();
                        }

                        break;
                    case "4":
                        Console.WriteLine("Введите Угол/2 : ");
                        string Anglee = Console.ReadLine();
                        Console.WriteLine("Введите начальное значение Х : ");
                        string Xbegine = Console.ReadLine();
                        Console.WriteLine("Введите конечное значение  Х : ");
                        string Xendinge = Console.ReadLine();
                        Console.WriteLine("Введите начальное значение Y : ");
                        string Ybegine = Console.ReadLine();
                        Console.WriteLine("Введите конечное значение  Y : ");
                        string Yendinge = Console.ReadLine();
                        Console.WriteLine("Введите шаг X : ");
                        string Step_X = Console.ReadLine();
                        Console.WriteLine("Введите шаг Y : ");
                        string Step_Y = Console.ReadLine();
                        double Step_Tie = Double.Parse(Step_X);
                        double Step_TiYe = double.Parse(Step_Y);
                        double MinEGBe = 12000; string Xdire = ""; string Ydire = "";
                        //double Step_Tie = Math.Abs(Math.Cos((Double.Parse(Anglee) * Math.PI) / 180)) * 2.9465 * Math.Sqrt(3) * 0.5;
                        //double Step_TiYe = Math.Abs(Math.Sin((Double.Parse(Anglee) * Math.PI) / 180) * 4.7867);

                        //string[] countangle = { "4.7", "6.6", "8.2", "10.9", "13.9", "15", "16.15", "21.05", "22.4" };

                        //for (int a = 0; a < countangle.Length; a++)
                        //{
                        //string Angle = countangle[a];

                        DirectoryInfo NewDire = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee);
                        if (!NewDire.Exists)
                        {
                            NewDire.Create();
                        }
                        DirectoryInfo NewDir22e = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee + "/" + Xbegine + "_" + Ybegine);
                        if (!NewDir22e.Exists)
                        {
                            NewDir22e.Create();
                        }
                        string TEXTMAsse = "Gbe SizeX SizeY  ";
                        Console.WriteLine("Шаг по Х : " + Step_Tie);
                        for (double xce = double.Parse(Xbegine); xce <= double.Parse(Xendinge); xce += Step_Tie)
                        {
                            Console.WriteLine("Осталось : " + (((xce - double.Parse(Xbegine)) / (double.Parse(Xendinge) - double.Parse(Xbegine))) * 100) + " %");
                            DirectoryInfo NewDir2e = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee + "/" + xce + "_" + Ybegine);
                            if (!NewDir2e.Exists)
                            {
                                NewDir2e.Create();
                            }
                            using (StreamWriter xyzdatae = File.AppendText(@"" + MyGlobals.GlobalPath + "/polyX" + Anglee + "" + xce + "" + double.Parse(Ybegine) + ".txt"))
                            {
                                xyzdatae.Write("box " + xce + " " + double.Parse(Ybegine) + " 9.5734\n" +
                                            "node 0.5*box 0.25*box 0.5*box 0° 0° -" + Anglee + "°\n" +
                                            "node 0.5*box 0.75*box 0.5*box 0° 0° " + Anglee + "°"); ;
                            }
                            CalcAtomsk(Anglee, xce, double.Parse(Ybegine));
                            CreatLMP_EGB(Anglee, xce, double.Parse(Ybegine));
                            CalcLammps3("Ti_HCP_" + Anglee, xce, double.Parse(Ybegine));
                            string ReadLogLammps = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee + "/log.gbe_X_" + xce + "_Y" + Ybegine);
                            string Size_Y = ReadLogLammps.Substring(ReadLogLammps.IndexOf("Ysize =") + 23, 2);
                            string Size_X = ReadLogLammps.Substring(ReadLogLammps.IndexOf("Xsize =") + 23, 2);
                            //Console.WriteLine(Size_X);
                            //Console.WriteLine(Size_Y);
                            string ReadLogGbe = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee + "/log.gbe_X_" + xce + "_Y" + Ybegine);
                            string GBE = ReadLogGbe.Substring(ReadLogGbe.IndexOf("GrainBoundaryEnergy =") + 64, 10);

                            TEXTMAsse = TEXTMAsse.Insert(16, "\n" + GBE + " " + xce + " " + Ybegine);
                            //////////////////////////////////////ENDING////////////////////////////////////////
                            //DelDir(Angle,xc, int.Parse(Ybegin));
                            if (MinEGBe > Convert.ToDouble(GBE))
                            {
                                MinEGBe = Convert.ToDouble(GBE); Xdire = Convert.ToString(xce); Ydire = Ybegine;
                            }
                            File.Delete(@"" + MyGlobals.GlobalPath + "/polyX" + Anglee + "" + xce + "" + double.Parse(Ybegine) + ".txt");
                            File.Delete(@"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + Anglee + "" + xce + "" + double.Parse(Ybegine));
                            for (double yce = double.Parse(Ybegine) + Step_Tie; yce <= double.Parse(Yendinge); yce += Step_TiYe)
                            //Parallel.For(int.Parse(Ybegin) + int.Parse(Step_Ti), int.Parse(Yending), yc =>
                            {
                                DirectoryInfo NewDir3e = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee + "/" + xce + "_" + yce);
                                if (!NewDir3e.Exists)
                                {
                                    NewDir3e.Create();
                                }
                                using (StreamWriter xyzdatae = File.AppendText(@"" + MyGlobals.GlobalPath + "/polyX" + Anglee + "" + xce + "" + yce + ".txt"))
                                {
                                    xyzdatae.Write("box " + xce + " " + yce + " 9.5734\n" +
                                                   "node 0.5*box 0.25*box 0.5*box 0° 0° -" + Anglee + "°\n" +
                                                   "node 0.5*box 0.75*box 0.5*box 0° 0° " + Anglee + "°");
                                }
                                CalcAtomsk(Anglee, xce, yce);
                                CreatLMP_EGB(Anglee, xce, yce);
                                CalcLammps3("Ti_HCP_" + Anglee, xce, yce);
                                string ReadLogLammpsY = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee + "/log.gbe_X_" + xce + "_Y" + yce);
                                string Size_YY = ReadLogLammpsY.Substring(ReadLogLammpsY.IndexOf("Ysize =") + 23, 2);
                                string Size_XY = ReadLogLammpsY.Substring(ReadLogLammpsY.IndexOf("Xsize =") + 23, 2);
                                //Console.WriteLine(Size_XY);
                                //Console.WriteLine(Size_YY);
                                string ReadLogGbeY = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + Anglee + "/log.gbe_X_" + xce + "_Y" + yce);
                                string GBEY = ReadLogGbeY.Substring(ReadLogGbeY.IndexOf("GrainBoundaryEnergy =") + 64, 10);
                                TEXTMAsse = TEXTMAsse.Insert(16, "\n" + GBEY + " " + xce + " " + yce);
                                if (MinEGBe > Convert.ToDouble(GBEY))
                                {
                                    MinEGBe = Convert.ToDouble(GBEY); Xdire = Convert.ToString(xce); Ydire = Convert.ToString(yce);
                                }
                                File.Delete(@"" + MyGlobals.GlobalPath + "/polyX" + Anglee + "" + xce + "" + yce + ".txt");
                                File.Delete(@"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + Anglee + "" + xce + "" + yce);
                            }
                            //Console.WriteLine("Осталось : " + ((1 - ((double.Parse(Xending) - xc) / (double.Parse(Xending) - xc))) * 100));
                        }
                        //);
                        Console.WriteLine("Минимальное значение ЭНЕРГИИ GB : " + MinEGBe); Console.WriteLine("Ширина блока = " + Xdire); Console.WriteLine("Высота блока = " + Ydire);

                        File.WriteAllText(@"" + MyGlobals.GlobalPath + "/polyX.txt", string.Empty);
                        using (StreamWriter olltxt = File.AppendText(@"" + MyGlobals.GlobalPath + "//polyX.txt"))
                        {
                            olltxt.Write("box " + Xdire + " " + Ydire + " 9.5734\n" +
                                "node 0.5*box 0.25*box 0.5*box 0° 0° -" + Anglee + "°\n" +
                                "node 0.5*box 0.75*box 0.5*box 0° 0° " + Anglee + "°");
                        }
                        AtomskCalc();
                        CalcLammps("Ti_Dump");

                        TEXTMAsse = TEXTMAsse.Replace("Gbe SizeX SizeY ", "");
                        File.WriteAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/DATA/Data_" + Anglee + ".txt", TEXTMAsse);
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///////////////////////////////////////////////////GNUPLOT/////////////////////////////////////////////////////////
                        GnuPlot.WriteLine("reset");
                        GnuPlot.Set("view map");
                        GnuPlot.Set("pm3d at b map");
                        GnuPlot.Set("dgrid3d 500,500,2");
                        GnuPlot.Set("xlabel \"Upscale to X(A)\"");
                        GnuPlot.Set("ylabel \"Upscale to Y(A)\"");
                        GnuPlot.Set("term png");
                        GnuPlot.Set("output \"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/DATA/Plot_3D_A_" + Anglee + ".png\"");
                        GnuPlot.SPlot(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/DATA/Data_" + Anglee + ".txt", "u 2:3:1 with image");
                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        DelDir(Anglee);
                        DirectoryInfo DeleteMine = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Test_MinimumEGB/");

                        foreach (FileInfo file in DeleteMine.GetFiles())
                        {
                            file.Delete();
                        }
                        break;
                    case "5":

                        //string[] countangle = { "7.16", "10.68", "15.08", "22.73", "28.31", "32.15", "38.94", "43.31", "48.53"
                        string[] countangle = { "56.46", "62.06","70.53","75.15", "82.45" , "83.95","90"};
                        Console.WriteLine("Введите начальное значение Х : ");
                        string Xbgn = Console.ReadLine();
                        Console.WriteLine("Введите конечное значение  Х : ");
                        string Xend = Console.ReadLine();
                        Console.WriteLine("Введите начальное значение Y : ");
                        string Ybgn = Console.ReadLine();
                        Console.WriteLine("Введите конечное значение  Y : ");
                        string Yend = Console.ReadLine();
                        Console.WriteLine("Введите шаг : ");
                        string Stp = "1";
                        double Egb_min = 12000; string Xdirect = ""; string Ydirect = "";
                        //double Step_Ti = Math.Abs(Math.Cos((Double.Parse(Angle)*Math.PI)/180)) * 2.9465 * Math.Sqrt(3)*0.5;
                        //double Step_TiY =Math.Abs(Math.Sin((Double.Parse(Angle)*Math.PI)/180) * 4.7867);


                        for (int a = 0; a < countangle.Length; a++)
                        {
                            Egb_min = 12000;
                            //string Angle = countangle[a];

                            DirectoryInfo NewDirectAngle = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + countangle[a]);
                            if (!NewDirectAngle.Exists)
                            {
                                NewDirectAngle.Create();
                            }
                            DirectoryInfo NewDirectionXY = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + countangle[a] + "/" + Xbgn + "_" + Ybgn);
                            if (!NewDirectionXY.Exists)
                            {
                                NewDirectionXY.Create();
                            }
                            string Text = "Gbe SizeX SizeY  ";
                            //Console.WriteLine("Шаг по Х : " + Step_Ti);
                            //for (double xc = double.Parse(Xbegin); xc <= double.Parse(Xending); xc += Step_Ti)
                            Parallel.For(int.Parse(Xbgn), int.Parse(Xend), xc =>
                            {
                                //Console.WriteLine("Осталось : " + (((xc- double.Parse(Xbegin)) /(double.Parse(Xending) - double.Parse(Xbegin)))*100) + " %");
                                DirectoryInfo NewDirect = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/Atomsk_xyz/" + countangle[a] + "/" + xc + "_" + Ybgn);
                                if (!NewDirect.Exists)
                                {
                                    NewDirect.Create();
                                }
                                using (StreamWriter xyzdata_4 = File.AppendText(@"" + MyGlobals.GlobalPath + "/polyX" + countangle[a] + "" + xc + "" + double.Parse(Ybgn) + ".txt"))
                                {
                                    xyzdata_4.Write("box " + xc + " " + double.Parse(Ybgn) + " 9.5734\n" +
                                                "node 0.5*box 0.25*box 0.5*box 0° 0° -" + countangle[a] + "°\n" +
                                                "node 0.5*box 0.75*box 0.5*box 0° 0° " + countangle[a] + "°"); ;
                                }
                                CalcAtomsk(countangle[a], xc, double.Parse(Ybgn));
                                CreatLMP_EGB(countangle[a], xc, double.Parse(Ybgn));
                                CalcLammps3("Ti_HCP_" + countangle[a], xc, double.Parse(Ybgn));
                                string ReadLogLammps = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/Atomsk_xyz/" + countangle[a] + "/log.gbe_X_" + xc + "_Y" + Ybgn);
                                string Size_Y = ReadLogLammps.Substring(ReadLogLammps.IndexOf("Ysize =") + 23, 2);
                                string Size_X = ReadLogLammps.Substring(ReadLogLammps.IndexOf("Xsize =") + 23, 2);

                                string ReadLogGbe = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/Atomsk_xyz/" + countangle[a] + "/log.gbe_X_" + xc + "_Y" + Ybgn);
                                string GBE = ReadLogGbe.Substring(ReadLogGbe.IndexOf("GrainBoundaryEnergy =") + 64, 10);

                                Text = Text.Insert(16, "\n" + GBE + " " + xc + " " + Ybgn);
                                //////////////////////////////////////ENDING////////////////////////////////////////
                                //DelDir(Angle,xc, int.Parse(Ybegin));
                                if (Egb_min > Convert.ToDouble(GBE))
                                {
                                    Egb_min = Convert.ToDouble(GBE); Xdirect = Convert.ToString(xc); Ydirect = Ybgn;
                                }
                                File.Delete(@"" + MyGlobals.GlobalPath + "/polyX" + countangle[a] + "" + xc + "" + double.Parse(Ybgn) + ".txt");
                                File.Delete(@"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + countangle[a] + "" + xc + "" + double.Parse(Ybgn));


                                Console.WriteLine("Данные по энергии: Позиция X = " + xc + " Позиция Y = " + Ybgn + " Egb = " + GBE + " MinEgm = " + Egb_min);

                                //for (double yc = double.Parse(Ybegin) + double.Parse(Step_Ti); yc <= double.Parse(Yending); yc += double.Parse(Step_Ti))
                                Parallel.For((int)(double.Parse(Ybgn) + double.Parse(Stp)), (int)double.Parse(Yend), yc =>
                                {
                                    DirectoryInfo NewDirect3 = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/Atomsk_xyz/" + countangle[a] + "/" + xc + "_" + yc);
                                    if (!NewDirect3.Exists)
                                    {
                                        NewDirect3.Create();
                                    }
                                    using (StreamWriter xyzdata = File.AppendText(@"" + MyGlobals.GlobalPath + "/polyX" + countangle[a] + "" + xc + "" + yc + ".txt"))
                                    {
                                        xyzdata.Write("box " + xc + " " + yc + " 9.5734\n" +
                                                          "node 0.5*box 0.25*box 0.5*box 0° 0° -" + countangle[a] + "°\n" +
                                                          "node 0.5*box 0.75*box 0.5*box 0° 0° " + countangle[a] + "°");
                                    }
                                    CalcAtomsk(countangle[a], xc, yc);
                                    CreatLMP_EGB(countangle[a], xc, yc);
                                    CalcLammps3("Ti_HCP_" + countangle[a], xc, yc);
                                    string ReadLogLammpsY = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/Atomsk_xyz/" + countangle[a] + "/log.gbe_X_" + xc + "_Y" + yc);
                                    string Size_YY = ReadLogLammpsY.Substring(ReadLogLammpsY.IndexOf("Ysize =") + 23, 2);
                                    string Size_XY = ReadLogLammpsY.Substring(ReadLogLammpsY.IndexOf("Xsize =") + 23, 2);

                                    string ReadLogGbeY = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/Atomsk_xyz/" + countangle[a] + "/log.gbe_X_" + xc + "_Y" + yc);
                                    string GBEY = ReadLogGbeY.Substring(ReadLogGbeY.IndexOf("GrainBoundaryEnergy =") + 64, 10);
                                    Text = Text.Insert(16, "\n" + GBEY + " " + xc + " " + yc);
                                    if (Egb_min > Convert.ToDouble(GBEY))
                                    {
                                        Egb_min = Convert.ToDouble(GBEY); Xdirect = Convert.ToString(xc); Ydirect = Convert.ToString(yc);
                                    }
                                    File.Delete(@"" + MyGlobals.GlobalPath + "/polyX" + countangle[a] + "" + xc + "" + yc + ".txt");
                                    File.Delete(@"" + MyGlobals.GlobalPath + "/in.Ti_HCP_" + countangle[a] + "" + xc + "" + yc);
                                    Console.WriteLine("Данные по энергии: Позиция X = " + xc + " Позиция Y = " + yc + " Egb = " + GBEY + " MinEgm = " + Egb_min);
                                });
                                //Console.WriteLine("Осталось : " + ((1 - ((double.Parse(Xending) - xc) / (double.Parse(Xending) - xc))) * 100));
                            });
                            Console.WriteLine("Минимальное значение ЭНЕРГИИ GB : " + Egb_min); Console.WriteLine("Ширина блока = " + Xdirect); Console.WriteLine("Высота блока = " + Ydirect);

                            using (StreamWriter write_data = File.AppendText(@"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/DATA/Data_All_Angle.txt"))
                            {
                                write_data.WriteLine((Double.Parse(countangle[a]) * 2) + " " + Egb_min);
                            }

                            Text = Text.Replace("Gbe SizeX SizeY ", "");
                            File.WriteAllText(@"" + MyGlobals.GlobalPath + "/TI/Mpi_Ti/DATA/Data_4_" + countangle[a] + ".txt", Text);
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            ///////////////////////////////////////////////////GNUPLOT/////////////////////////////////////////////////////////
                            GnuPlot.WriteLine("reset");
                            GnuPlot.Set("view map");
                            GnuPlot.Set("pm3d at b map");
                            GnuPlot.Set("dgrid3d 500,500,2");
                            GnuPlot.Set("xlabel \"Upscale to X(A)\"");
                            GnuPlot.Set("ylabel \"Upscale to Y(A)\"");
                            GnuPlot.Set("term png");
                            GnuPlot.Set("output \"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/DATA/Plot_4_A_" + countangle[a] + ".png\"");
                            GnuPlot.SPlot(@"" + MyGlobals.GlobalPath + "/Ti/Mpi_Ti/DATA/Data_4_" + countangle[a] + ".txt", "u 3:2:1 with image");
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            DelDir(countangle[a]);
                            DirectoryInfo DeleteMinin = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/TI/Test_MinimumEGB/");

                            foreach (FileInfo file in DeleteMinin.GetFiles())
                            {
                                file.Delete();
                            }

                        }
                        break;
                    case "6":
                        string[] AnglesHCP = { "64.3" };
                        Parallel.For(0, AnglesHCP.Length, e =>
                        {
                            string VFE_data_simpl = @"" + MyGlobals.GlobalPath + "/dumpset" + AnglesHCP[e] + "_500_500_500";
                            File.WriteAllText(VFE_data_simpl, string.Empty);
                            using (StreamWriter sw_simpl = File.AppendText(VFE_data_simpl))
                            {
                                sw_simpl.Write("shell cd  VFE/${METALL}/${Angleses} \ndump 1 all custom 400 dump.defCOORD.2.* x y z ");
                            }
                            Creat_VFE_LMP(AnglesHCP[e], "500", "500", "500");
                            CalcLammps("VFE_" + AnglesHCP[e] + "_Ti_" + 500 + "_" + 500 + "_" + 500 + "");

                            int countline = File.ReadAllLines(MyGlobals.GlobalPath + "/VFE/Ti/" + AnglesHCP[e] + "/dump.defCOORD.2.0").Length;
                            Console.WriteLine("Количество Строк = " + countline);
                            string[] lines = File.ReadAllLines(MyGlobals.GlobalPath + "/VFE/Ti/" + AnglesHCP[e] + "/dump.defCOORD.2.0").Skip(9).Take(countline - 8).ToArray();
                            foreach (var i in lines)
                                Console.WriteLine(i);
                            Console.WriteLine("Количество строк " + lines.Length);

                            File.WriteAllText(VFE_data_simpl, string.Empty); // Очищение текстового файла

                            string ReadLog = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/log.VFE_" + AnglesHCP[e] + "_X_500_Y_500_Z_500");
                            string Size_Y = ReadLog.Substring(ReadLog.IndexOf("Ysize =") + 23, 4);
                            Console.WriteLine("Из лог файла данные по размеру : " + Size_Y);

                            double Ysize = (Double.Parse(Size_Y) / 4);

                            // преобразование в двумерный массив
                            int n = lines.Length;
                            int m = 3;
                            double[,] twod = new double[n, m];
                            for (int i = 0; i < n; i++)
                            {
                                string[] vari = lines[i].Split(' ');
                                for (int j = 0; j < m; j++)
                                {
                                    twod[i, j] = Double.Parse(vari[j]);

                                }
                            }
                            // Удаление строк с условием и запись в другой файл
                            for (int i = 0; i < lines.Length; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    if (twod[i, 0] > 30 && twod[i, 0] < 44 && twod[i, 1] < 3*Ysize && twod[i, 1] > Ysize && twod[i, 2] > 23 && twod[i, 2] < 27)
                                    {
                                        string rightvfeq = @"" + MyGlobals.GlobalPath + "/VFE/Ti/DATA_VFE/DATA_Ti_VFE_" + AnglesHCP[e];
                                        using (StreamWriter sw = File.AppendText(rightvfeq))
                                        {
                                            sw.Write(twod[i, j] + " ");
                                        }
                                    }
                                }
                                string rightvfe = @"" + MyGlobals.GlobalPath + "/VFE/Ti/DATA_VFE/DATA_Ti_VFE_" + AnglesHCP[e];
                                using (StreamWriter sw = File.AppendText(rightvfe))
                                {
                                    sw.WriteLine();
                                }
                            }
                            // ЗАПИСЬ В ДРУГОЙ ФАЙЛ С УДАЛЕНИЕМ ПУСТЫХ СТРОК
                            string[] mass = File.ReadAllLines(@"" + MyGlobals.GlobalPath + "/VFE/Ti/DATA_VFE/DATA_Ti_VFE_" + AnglesHCP[e]);
                            StreamWriter NewFile = File.CreateText(@"" + MyGlobals.GlobalPath + "/VFE/Ti/DATA_VFE/DATA_XYZ_VFE_" + AnglesHCP[e] + ".txt");
                            for (int i = 0; i < mass.Length; i++)
                            {
                                if (mass[i] != "")
                                {
                                    NewFile.WriteLine(mass[i]);
                                }
                            }
                            NewFile.Close();

                            FileInfo BetaVfeData = new FileInfo(@"" + MyGlobals.GlobalPath + "/VFE/Ti/DATA_VFE/DATA_Ti_VFE_" + AnglesHCP[e]);
                            BetaVfeData.Delete();
                            // Конец первой части: в остатке файл с данными координат
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Взято с старого скрипта.
                            DirectoryInfo Vfe_m = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/VFE/Ti/" + AnglesHCP[e] + "/");
                            if (!Vfe_m.Exists)
                            {
                                Vfe_m.Create();
                            }
                            string path_vfe_simpl = @"" + MyGlobals.GlobalPath + "/VFE/Ti/DATA_Ti_VFE_" + AnglesHCP[e] + ".txt";

                            string VFE_path_simpl = @"" + MyGlobals.GlobalPath + "/VFE/Ti/DATA_VFE/DATA_XYZ_VFE_" + AnglesHCP[e] + ".txt";

                            File.Delete(@"" + MyGlobals.GlobalPath + "/in.VFE_" + AnglesHCP[e] + "_Ti_500_500_500");
                            File.Delete(@"" + MyGlobals.GlobalPath + "/log.VFE_" + AnglesHCP[e] + "_X_500_Y_500_Z_500");
                            File.Delete(@"" + MyGlobals.GlobalPath + "/VFE/Ti/" + AnglesHCP[e] + "/dump.defCOORD.2.0"); //VFE/${METALL}/${Sigma}

                            string[] Mass_simpl = File.ReadAllLines(VFE_path_simpl, Encoding.Default); // Запись строк в массив

                            Console.WriteLine("Рассчет Ti Сигмы :" + AnglesHCP[e] + " Потенциал: eam/fs");
                            //Parallel.For(0, Mass_simpl.Length, i => 
                            for (int i = 0; i < Mass_simpl.Length; i++)
                            {
                                string[] posY_simpl = Mass_simpl[i].Split();
                                //using (StreamWriter sw_simpl = File.AppendText(VFE_data_simpl))
                                //{
                                //    sw_simpl.Write("variable pos string '" + Mass_simpl[i] + "'");
                                //}
                                string VFE_dataDump = @"" + MyGlobals.GlobalPath + "/dumpset" + AnglesHCP[e] + "_" + posY_simpl[0] + "_" + posY_simpl[1] + "_" + posY_simpl[2] + "";
                                File.WriteAllText(VFE_dataDump, string.Empty);
                                using (StreamWriter sw_simpl = File.AppendText(VFE_dataDump))
                                {
                                    sw_simpl.Write("shell cd  VFE/${METALL}/${Angleses} \n dump 1 all custom 400 dump.VFE_${METALL}_X_" + posY_simpl[0] + "_Y_" + posY_simpl[1] + "_Z_" + posY_simpl[2] + " id xs ys zs c_csym c_eng");
                                }

                                Creat_VFE_LMP(AnglesHCP[e], posY_simpl[0], posY_simpl[1], posY_simpl[2]);
                                CalcLammpsHIDE("VFE_" + AnglesHCP[e] + "_Ti_" + posY_simpl[0] + "_" + posY_simpl[1] + "_" + posY_simpl[2] + "");
                                string VFE_vol_simpl = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/log.VFE_" + AnglesHCP[e] + "_X_" + posY_simpl[0] + "_Y_" + posY_simpl[1] + "_Z_" + posY_simpl[2]);
                                string V_simpl = VFE_vol_simpl.Substring(VFE_vol_simpl.IndexOf("Vacancy formation energy =") + 62, 6);
                                using (StreamWriter sw_vfe_simpl = File.AppendText(path_vfe_simpl))
                                {
                                    sw_vfe_simpl.WriteLine(V_simpl + " " + posY_simpl[1]);
                                }
                                string oldFileName = @"" + MyGlobals.GlobalPath + "/VFE/Ti/" + AnglesHCP[e] + "/dump.VFE_Ti_X_" + posY_simpl[0] + "_Y_" + posY_simpl[1] + "_Z_" + posY_simpl[2];
                                string newFileName = @"" + MyGlobals.GlobalPath + "/VFE/Ti/" + AnglesHCP[e] + "/dump.VFE_Ti_Enrg_" + V_simpl + "_X_" + posY_simpl[0] + "_Y_" + posY_simpl[1] + "_Z_" + posY_simpl[2]; ; // Full path of new file
                                if (File.Exists(oldFileName))
                                {
                                    File.Copy(oldFileName, newFileName, true);
                                    File.Delete(oldFileName);
                                }

                                Console.WriteLine("VFE(Ev) : " + V_simpl + "   | Позиции по Y :" + posY_simpl[1]);
                                Console.WriteLine("Осталось: " + i + " из " + Mass_simpl.Length + " ||  В процентах : " + (i * 100) / Mass_simpl.Length + "%");

                                File.Delete(@"" + MyGlobals.GlobalPath + "/in.VFE_" + AnglesHCP[e] + "_Ti_" + posY_simpl[0] + "_" + posY_simpl[1] + "_" + posY_simpl[2] + "");
                                File.Delete(@"" + MyGlobals.GlobalPath + "/log.VFE_" + AnglesHCP[e] + "_X_" + posY_simpl[0] + "_Y_" + posY_simpl[1] + "_Z_" + posY_simpl[2]);
                                File.Delete(@"" + MyGlobals.GlobalPath + "/dumpset" + AnglesHCP[e] + "_" + posY_simpl[0] + "_" + posY_simpl[1] + "_" + posY_simpl[2] + "");

                            };
                            File.Delete(@"" + MyGlobals.GlobalPath + "/dumpset" + AnglesHCP[e] + "_" + 500 + "_" + 500 + "_" + 500 + "");
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            ///////////////////////////////////////////////////GNUPLOT/////////////////////////////////////////////////////////
                            GnuPlot.Set("xlabel \"Distance from GB (nm)\"");
                            GnuPlot.Set("ylabel \"Vacancy Formation Energy(Ev)\"");
                            GnuPlot.Set("term png");
                            GnuPlot.Set("output \"" + Vfe_m + "/Plot_VFE_Ti_A" + AnglesHCP[e] + ".png\"");
                            GnuPlot.Plot(path_vfe_simpl, "u 2:1 w p pt 13 ps 1.2 lc rgb \"black\"");
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        });

                        break;
                    case "7":
                        string[] Sigma = { "64.3" };
                        for (int u = 0; u < Sigma.Length; u++)
                        {

                            CreatIFELMP(Sigma[u], 0, 0);
                            CalcLammpsHIDE("IFE_Ti_00");

                            string ReadLog = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/log.IFE_X_0_Y0");
                            string Size_Y = ReadLog.Substring(ReadLog.IndexOf("Ysize =") + 23, 2);
                            ///string Size_X = ReadLog.Substring(ReadLog.IndexOf("Xsize =") + 23, 6);
                            //string SigmaN = ReadLog.Substring(ReadLog.IndexOf("include  Sigma") + 14, 9);
                            //Console.WriteLine("Из лог файла данные по размеру : " + Size_Y);
                            //Console.WriteLine("Из лог файла данные по Сигме : " + SigmaN);
                            //double Ysize = double.Parse(Size_Y);
                            int Ysize = int.Parse(Size_Y) / 4;
                            int Xsize = 40;
                            Console.WriteLine(Ysize);
                            string TEXTMAssIFE = "";
                            // Конец первой части: в остатке файл с данными координат
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            DirectoryInfo Ife_m = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/IFE/IFE_Result/");
                            if (!Ife_m.Exists)
                            {
                                Ife_m.Create();
                            }
                            string path_ife_simpl = Ife_m + "/DATA_Ti_IFE_" + Sigma[u];
                            Console.WriteLine("Рассчет Ti угла :" + Sigma[u] + " Потенциал: ");

                            File.Delete(@"" + MyGlobals.GlobalPath + "/in.IFE_Ti_00");
                            File.Delete(@"" + MyGlobals.GlobalPath + "/log.IFE_Ti_0_Y0");
                            File.Delete(@"" + MyGlobals.GlobalPath + "/IFE/Ti/" + Sigma[u] + "/dump.TiIFEdef_X_0_Y_0");

                            //string Path = @"" + MyGlobals.GlobalPath + "IFE/W/"+Sigma"; //C:/Program Files/LAMMPS/bin
                            //Directory.CreateDirectory(Path + "S" + N + "\\Overlap_" + start);
                            //DirectoryInfo source = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/Dump/");
                            //DirectoryInfo destin = new DirectoryInfo(Path + "S" + N + "\\OverLap_" + start + "\\");

                            //foreach (var item in source.GetFiles())
                            //{
                            //    item.CopyTo(destin + item.Name, true);
                            //}
                            //DirectoryInfo dirInfo = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/Dump/");

                            //for (double i = 37.75; i < 45; i+=0.25)
                            Parallel.For(40, 50, i =>
                            {
                                //File.WriteAllText(IFE_data_simpl, string.Empty); // Очищение текстового файла

                                //CreatIFELMP(Sigma[u], i, 0);
                                //    CalcLammpsMpiHide("IFE_W_" + i + "0");
                                //   string IFE_vol_simpl = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/log.IFE_X_" + i + "_Y0");
                                //  string Energy_SIA = IFE_vol_simpl.Substring(IFE_vol_simpl.IndexOf("Interstitial formation energy =") + 72, 6);
                                //  TEXTMAss = TEXTMAss.Insert(0, "\n" + Energy_SIA + " " + i + " " + 0);
                                //   Console.WriteLine("IFE(Ev) : " + Energy_SIA + "   | Позиции по X :" + i + " | Y : 0");
                                //   DirectoryInfo Ife_ENG = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/IFE/IFE_Result/"+Sigma[u]+"/" + Math.Round(double.Parse(Energy_SIA), 1) + "");
                                //   if (!Ife_ENG.Exists)
                                //   {
                                //       Ife_ENG.Create();
                                //   }
                                //   string Path2 = @"" + MyGlobals.GlobalPath + "/IFE/IFE_Result/" + Sigma[u] + "/" + Math.Round(double.Parse(Energy_SIA), 1) + "/dump.SIA_"+ Energy_SIA+"_X_" + i + "_Y_0";
                                //   string Path = @"" + MyGlobals.GlobalPath + "/IFE/W/" + Sigma[u] + "/dump.WIFEdef_X_" + i + "_Y_0"; //dump.${METALL}IFEdef_X_"+cx+"_Y_"+cy
                                //File.Move(Path, Path2);
                                // Очистка
                                //File.Delete(@"" + MyGlobals.GlobalPath + "/in.IFE_W_" + i + "0");
                                //File.Delete(@"" + MyGlobals.GlobalPath + "/log.IFE_X_" + i + "_Y0");
                                for (double e = 30; e <= 60; e += 0.5)
                                //Parallel.For(-Ysize, Ysize, e =>
                                {

                                    CreatIFELMP(Sigma[u], i, e);
                                    CalcLammpsHIDE("IFE_Ti_" + i + "" + e);
                                    string IFE_vol = File.ReadAllText(@"" + MyGlobals.GlobalPath + "/log.IFE_X_" + i + "_Y" + e + "");
                                    string EnergyY_SIA = IFE_vol.Substring(IFE_vol.IndexOf("Interstitial formation energy =") + 72, 6);
                                    TEXTMAssIFE = TEXTMAssIFE.Insert(0, "\n" + EnergyY_SIA + " " + i + " " + e);
                                    Console.WriteLine("IFE(Ev) : " + EnergyY_SIA + "   | Позиции по X :" + i + " | Y : " + e);
                                    //Console.WriteLine("Осталось: " + e + " из " + Ysize + " ||  В процентах : " + (e * 100) / Ysize + "%");

                                    DirectoryInfo Ife_ENGY = new DirectoryInfo(@"" + MyGlobals.GlobalPath + "/IFE/IFE_Result/" + Sigma[u] + "/" + Math.Round(double.Parse(EnergyY_SIA), 1) + "");
                                    if (!Ife_ENGY.Exists)
                                    {
                                        Ife_ENGY.Create();
                                    }
                                    string Path2Y = @"" + MyGlobals.GlobalPath + "/IFE/IFE_Result/" + Sigma[u] + "/" + Math.Round(double.Parse(EnergyY_SIA), 1) + "/dump.SIA_" + EnergyY_SIA + "_X_" + i + "_Y_" + e;
                                    string PathY = @"" + MyGlobals.GlobalPath + "/IFE/Ti/" + Sigma[u] + "/dump.IFEdef_X_" + i + "_Y_" + e; //dump.${METALL}IFEdef_X_"+cx+"_Y_"+cy
                                    File.Move(PathY, Path2Y);

                                    File.Delete(@"" + MyGlobals.GlobalPath + "/in.IFE_Ti_" + i + "" + e);
                                    File.Delete(@"" + MyGlobals.GlobalPath + "/log.IFE_X_" + i + "_Y" + e + "");
                                }
                            });
                           // TEXTMAss = TEXTMAsss.Replace("IFE XSize YSize", "");
                            File.WriteAllText(path_ife_simpl, TEXTMAssIFE);
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            ///////////////////////////////////////////////////GNUPLOT/////////////////////////////////////////////////////////
                            GnuPlot.Set("xlabel \"Distance from GB (nm)\"");
                            GnuPlot.Set("ylabel \"Interstitial Formation Energy(Ev)\"");
                            GnuPlot.Set("term png");
                            GnuPlot.Set("output \"" + Ife_m + "/Plot_IFE_Ti_S" + Sigma[u] + ".png\"");
                            GnuPlot.Plot(path_ife_simpl, "u 3:1 w p pt 13 ps 1.2 lc rgb \"black\"");
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            ///
                        }
                        break;
                        Console.WriteLine("Завершить ? ('y' = да):");
                        string y = Console.ReadLine();
                        if (y == "y")
                        break;
                        else continue;
                }
            }
        }
    }
}
