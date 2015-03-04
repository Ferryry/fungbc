﻿
open Cpu

[<EntryPoint>]
let main argv = 
    
    
    let cpu = CPU()


    let program = [
                    0x3E; 0xDA;         // LD A, 0xDA
                    0xEA; 0xFA; 0x0;    // LD (0xFA), A
                    0xFA; 0xFA; 0x0     // LD A, (0xFA)
                    0x10;               // STOP
                  ]|> List.map uint8 |> List.toArray

    cpu.loadProgram program

    cpu.start ()

    cpu.printState ()

    cpu.printMemory 0x0 0xFF

    0
