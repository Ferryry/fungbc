﻿
open Cpu
open Mmu
open Rom
open Ram
open Interrupts
open IORegisters
open Clock
open Constants

[<EntryPoint>]
let main argv = 
    
    if argv.Length <> 1 then raise (System.Exception("Usage: fgbc <fgbc-file>"))

    let rom = LoadROMFromFGBC argv.[0]

    let ram = GBCRam()

    let systemClock = MutableClock(GBC_SYSTEM_CLOCK_FREQUENCY,0UL)

    let ioRegisters = IORegisters(systemClock)

    let mmu = MMU(rom,ram,ioRegisters)

    let cpu = CPU(mmu,ioRegisters,systemClock)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()

    cpu.Start ()

    stopWatch.Stop()

    printfn "CPU execution time: %d ms" (int stopWatch.Elapsed.TotalMilliseconds) 

    systemClock.Print ()

    cpu.Registers.Print ()

    mmu.PrintDump 0x0 0xFF

    mmu.PrintDump 0xFF00 0xFFFF

    0
