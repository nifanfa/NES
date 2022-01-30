using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NES
{
    public partial class NES : Form
    {
        FileStream fsOpenRom;
        Registers registers;
        MemoryMap memory;
        Mappers mappers;
        CPU cpu;
        GameRender gameRender;
        Input input;
        PPU ppu;

        #region Global/Local Variables
        public bool bolRunGame;
        public bool bolStartFrame, bolReset = true;
        int intFPS = 0;
        int intMaxCPUCycles = 29780 * 15;
        public int tsMaster;
        public int tsCpuNTSC;
        public int tsCpuPAL;
        public int tsPpu;
        public int cntScanline = -2;
        public int cntScanlineCycle;
        public int VBlankTime = 20 * 341 * 5;
        public byte MapperNumber;
        public bool rendering = false;
        #endregion

        public void runGame()
        {
            if (bolRunGame && !cpu.badOpCode)
            {   
                if (cpu.intTotalCpuCycles < intMaxCPUCycles)
                {
                    cpu.execOpCode();

                    ppu.RunPPU(cpu.intTotalCpuCycles);
                }

                if (ppu.bolReadyToRender)
                {
                    gameRender.WriteBitmap(ppu.byteBGFrame, ppu.BGColor);

                    ppu.bolReadyToRender = false;

                    cpu.intTotalCpuCycles = 0;
                    tsPpu = 0;

                    intFPS++;
                }
            }     
        }

        public void openROM(string strFileLoc)
        {
            //
            label1.Visible = false;

            fsOpenRom = new FileStream(strFileLoc, FileMode.Open);
            byte[] temp = new byte[16];

            fsOpenRom.Read(temp, 0, temp.Length);
            memory.byteMirror = (byte)(temp[6] & 0x01);

            mappers.MapperNumber = (byte)((temp[6] >> 0x04) | (temp[7] & 0xF0));
            memory.MapperNumber = mappers.MapperNumber;

            byte byteNumPRGBanks = temp[4];
            byte byteNumCHRBanks = temp[5];

            memory.memPRG = new byte[byteNumPRGBanks][];
            if (byteNumPRGBanks == 0x01)
            {
                fsOpenRom.Read(memory.memCPU, 0xC000, 0x4000);

                for (int i = 0x0000; i < 0x4000; i++)
                {
                    memory.memCPU[0x8000 + i] = memory.memCPU[0xC000 + i];
                }
            }
            else
            {
                for (int j = 0; j < memory.memPRG.Length; j++)
                {
                    memory.memPRG[j] = new byte[0x4000];
                }

                for (int k = 0; k < memory.memPRG.Length; k++)
                {
                    fsOpenRom.Read(memory.memPRG[k], 0, 0x4000);
                }

                for (int l = 0; l < 0x4000; l++)
                {
                    memory.memCPU[l + 0x8000] = memory.memPRG[0][l];
                    memory.memCPU[l + 0xC000] = memory.memPRG[memory.memPRG.Length - 1][l];
                }
            }

            if (byteNumCHRBanks != 0)
            {
                memory.memCHR = new byte[byteNumCHRBanks][];


                for (int x = 0; x < memory.memCHR.Length; x++)
                {
                    memory.memCHR[x] = new byte[0x2000];
                }

                for (int y = 0; y < memory.memCHR.Length; y++)
                {
                    fsOpenRom.Read(memory.memCHR[y], 0, 0x2000);
                }

                for (int z = 0; z < 0x2000; z++)
                {
                    memory.memPPU[z] = memory.memCHR[0][z];
                }
            }

            registers.regPC = memory.memCPU[0xFFFC] + memory.memCPU[0xFFFD] * 16 * 16;


            memory.memCPU[0x4016] = 0x40;
            memory.memCPU[0x4017] = 0x40;

            bolRunGame = true;
        }

        public void resetGame()
        {
            //
            label1.Visible = true;

            registers = new Registers();
            input = new Input();
            mappers = new Mappers();
            memory = new MemoryMap(registers, input, this, mappers);
            ppu = new PPU(memory, this);
            cpu = new CPU(memory, input, ppu, registers);

            gameRender = new GameRender(this);
            
            bolStartFrame = true;
            bolReset = true;
        }

        #region Keyboard Input
        public void NES_KeyDown(object sender, KeyEventArgs e)
        {
            char c = (char)(e.KeyCode);

            if (c == 'Q')        // A
            {
                input.joypadOne |= 0x01;
            }
            else if (c == 'E')   // B
            {
                input.joypadOne |= 0x02;
            }
            else if (c == 'Z')   // Select
            {
                input.joypadOne |= 0x04;
            }
            else if (c == 'C')   // Start
            {
                input.joypadOne |= 0x08;
            }
            else if (c == 'W')   // Up
            {
                input.joypadOne |= 0x10;
            }
            else if (c == 'S')   // Down
            {
                input.joypadOne |= 0x20;
            }
            else if (c == 'A')   // Left
            {
                input.joypadOne |= 0x40;
            }
            else if (c == 'D')   // Right
            {
                input.joypadOne |= 0x80;
            }
        }

        private void NES_KeyUp(object sender, KeyEventArgs e)
        {
            Keys c = e.KeyCode;

            if (c == Keys.Q)        // A
            {
                input.joypadOne &= 0xFE;
            }
            else if (c == Keys.E)   // B
            {
                input.joypadOne &= 0xFD;
            }
            else if (c == Keys.Z)   // Select
            {
                input.joypadOne &= 0xFB;
            }
            else if (c == Keys.C)   // Start
            {
                input.joypadOne &= 0xF7;
            }
            else if (c == Keys.W)   // Up
            {
                input.joypadOne &= 0xEF;
            }
            else if (c == Keys.S)   // Down
            {
                input.joypadOne &= 0xDF;
            }
            else if (c == Keys.A)   // Left
            {
                input.joypadOne &= 0xBF;
            }
            else if (c == Keys.D)   // Right
            {
                input.joypadOne &= 0x7F;
            }
        }
        #endregion

        private void tmrFPS_Tick(object sender, EventArgs e)
        {
            this.Text = ("FPS: ") + Convert.ToString(intFPS) + "  NameTable: " + String.Format("{0:x4}", ppu.nameTable) + "  Mapper #: " + Convert.ToString(MapperNumber)
                        + ", Bank: " + Convert.ToString(memory.MapperBank);
            intFPS = 0;
        }

        private void DragToOpenFile(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            openROM(files[0]);
        }

        private void DragEnterEvent(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;  
            }
        }

        public void NMIHandler()
        {
            cpu.NMIHandler(memory);
        }

        public void AddCPUCycles(int cycles)
        {
            cpu.intTotalCpuCycles += cycles * 15;
        }

        public void Present(Image image) 
        {
            pictureBox1.Image = image;
        }

        public NES()
        {
            InitializeComponent();
            resetGame();
        }
    }
}
