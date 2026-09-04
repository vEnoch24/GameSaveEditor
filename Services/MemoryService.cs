using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GameSaveEditor.Services
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerData
    {
        public int CurrentHealth;  // Offset 0
        public int MaxHealth;      // Offset 4
        public int Mana;           // Offset 8
        public int Gold;           // Offset 12
        public int Level;          // Offset 16
        public float WalkSpeed;    // Offset 20
        public float Overdrive;
        public float Materials;
        public float Skills_points;
        public float Stat_Points;
    }
    public class MemoryService
    {
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        public IntPtr OpenProcessHandle(int processId)
        {
            return OpenProcess(PROCESS_ALL_ACCESS, false, processId);
        }

        public IntPtr ResolvePointerAddress(IntPtr processHandle, IntPtr baseAddress, int[] offsets)
        {
            IntPtr currentAddress = baseAddress;
            byte[] buffer = new byte[8]; // 64-bit systems pointer sizing

            for (int i = 0; i < offsets.Length - 1; i++)
            {
                IntPtr addressToRead = currentAddress + offsets[i];
                if (!ReadProcessMemory(processHandle, addressToRead, buffer, buffer.Length, out _))
                    return IntPtr.Zero;

                currentAddress = (IntPtr)BitConverter.ToInt64(buffer, 0);
                if (currentAddress == IntPtr.Zero) return IntPtr.Zero;
            }
            return currentAddress + offsets[offsets.Length - 1];
        }

        public T ReadMemoryStruct<T>(IntPtr processHandle, IntPtr address) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            ReadProcessMemory(processHandle, address, buffer, size, out _);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T data = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return data;
        }

        public bool WriteMemoryStruct<T>(IntPtr processHandle, IntPtr address, T structValue) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(structValue, handle.AddrOfPinnedObject(), false);
            handle.Free();

            return WriteProcessMemory(processHandle, address, buffer, size, out _);
        }
    }



}
