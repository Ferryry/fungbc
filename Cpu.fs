﻿module Cpu

open Mmu
open Register
open Instruction


type CPU () =

    let mmu = MMU()
    
    let registers = RegisterSet()

    let mutable debugEnabled = false

    let rec execute () = 

        let A = registers.A
        let B = registers.B
        let C = registers.C
        let D = registers.D
        let E = registers.E
        let F = registers.F
        let H = registers.H
        let L = registers.L
        let AF = registers.AF
        let BC = registers.BC
        let DE = registers.DE
        let HL = registers.HL
        let SP = registers.SP
        let PC = registers.PC

        let instruction = decodeOpcode mmu PC.value

        printfn "Executing instruction: %s @ 0x%04X" (readable instruction) PC.value

        // Shorted versions of name -> register lookup functions
        let r8 = registers.from8Name
        let r8r8 r1 r2 = (r8 r1,r8 r2)
        let r16 = registers.from16Name

        // Quick PC manipulation
        let inline incPC offset = PC.value <- PC.value + (uint16 offset)
        let inline setPC address = PC.value <- address 
        
        match instruction with
        | NOP ->
            incPC 1
        | STOP ->
            incPC 0
        | LD_R8_R8 (r1,r2) ->
            let r1, r2 = r8r8 r1 r2
            r1.value <- r2.value
            incPC 1
        | LD_R8_D8 (r,d) ->
            (r8 r).value <- d
            incPC 2
        | LD_A16_R8 (a,r) ->
            mmu.write8 a ((r8 r).value)
            incPC 3
        | LD_R8_A16 (r,a) ->
            (r8 r).value <- mmu.read8 a
            incPC 3
        | INC_R8 (r) ->
            let r = r8 r
            r.value <- r.value + 1uy
            incPC 1
        | DEC_R8 (r) ->
            let r = r8 r
            r.value <- r.value - 1uy
            incPC 1
        | SWAP_R8 (r) ->
            let r = r8 r
            r.value <- ((r.value &&& 0xFuy) <<< 4) ||| ((r.value &&& 0xF0uy) >>> 4)
            incPC 2
        | SCF ->
            registers.F.C <- SET
            incPC 1
        | CCF ->
            registers.F.C <- CLEAR
            incPC 1
        | _ -> raise (System.Exception(sprintf "opcode <%O> not implemented" instruction))
        
        if instruction <> STOP then
            execute ()

    member this.enableDebug () = debugEnabled <- true

    member this.loadProgram (program: array<uint8>) =
        let baseAddress = 0us
        program |> Array.iteri (fun index b -> mmu.write8 (baseAddress + (uint16 index)) b)
    
    member this.printState () =
        let r = registers
        printfn @"CPU State:
            A  = 0x%02X
            B  = 0x%02X
            C  = 0x%02X
            D  = 0x%02X
            E  = 0x%02X
            F  = 0x%02X (Z = %d, N = %d, H = %d, C = %d)
            H  = 0x%02X
            L  = 0x%02X
            AF = 0x%04X
            BC = 0x%04X
            DE = 0x%04X
            HL = 0x%04X
            PC = 0x%04X
            SP = 0x%04X
        " r.A.value r.B.value r.C.value r.D.value r.E.value 
            r.F.value (bitStateToValue r.F.Z) (bitStateToValue r.F.N) (bitStateToValue r.F.H) (bitStateToValue r.F.C)
            r.H.value r.L.value r.AF.value r.BC.value
            r.DE.value r.HL.value r.PC.value r.SP.value

    member this.printMemory = mmu.printDump
    
    member this.reset () =
        registers.A.value <- 0x00uy
        registers.B.value <- 0x00uy
        registers.C.value <- 0x13uy
        registers.F.value <- 0xB0uy
        registers.DE.value <- 0x00D8us
        registers.HL.value <- 0x014Dus
        registers.SP.value <- 0xFFFEus
        registers.PC.value <- 0us


    member this.start () =
        this.reset()
        execute ()


