-- VHDL libraries necessary for the generated code to work. These libraries are included here instead of being managed separately in the Hardware framework so they can be more easily updated.

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

package TypeConversion is
	function SmartResize(input: unsigned; size: natural) return unsigned;
	function SmartResize(input: signed; size: natural) return signed;
	function ToUnsignedAndExpand(input: signed; size: natural) return unsigned;
end TypeConversion;
		
package body TypeConversion is

	-- The .NET behavior is different than that of resize() ("To create a larger vector, the new [leftmost] bit 
	-- positions are filled with the sign bit(ARG'LEFT). When truncating, the sign bit is retained along with the 
	-- rightmost part.") when casting to a smaller type: "If the source type is larger than the destination type, 
	-- then the source value is truncated by discarding its “extra” most significant bits. The result is then 
	-- treated as a value of the destination type." Thus we need to simply truncate when casting down.
	function SmartResize(input: unsigned; size: natural) return unsigned is
	begin
		if (size < input'LENGTH) then
			return input(size - 1 downto 0);
		else
			-- Resize() is supposed to work with little endian numbers: "When truncating, the sign bit is retained
            -- along with the rightmost part." for signed numbers and "When truncating, the leftmost bits are 
            -- dropped." for unsigned ones. See: http://www.csee.umbc.edu/portal/help/VHDL/numeric_std.vhdl
			return resize(input, size);
		end if;
	end SmartResize;

	function SmartResize(input: signed; size: natural) return signed is
	begin
		if (size < input'LENGTH) then
			return input(size - 1 downto 0);
		else
			return resize(input, size);
		end if;
	end SmartResize;

	function ToUnsignedAndExpand(input: signed; size: natural) return unsigned is
		variable result: unsigned(size - 1 downto 0);
	begin
		if (input >= 0) then
			return resize(unsigned(input), size);
		else 
			result := (others => '1');
			result(input'LENGTH - 1 downto 0) := unsigned(input);
			return result;
		end if;
	end ToUnsignedAndExpand;

end TypeConversion;


library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
		
package SimpleMemory is
	-- Data conversion functions:
	function ConvertUInt32ToStdLogicVector(input: unsigned(31 downto 0)) return std_logic_vector;
	function ConvertStdLogicVectorToUInt32(input : std_logic_vector) return unsigned;
		
	function ConvertBooleanToStdLogicVector(input: boolean) return std_logic_vector;
	function ConvertStdLogicVectorToBoolean(input : std_logic_vector) return boolean;
		
	function ConvertInt32ToStdLogicVector(input: signed(31 downto 0)) return std_logic_vector;
	function ConvertStdLogicVectorToInt32(input : std_logic_vector) return signed;
		
	function ConvertCharToStdLogicVector(input: character) return std_logic_vector;
	function ConvertStdLogicVectorToChar(input : std_logic_vector) return character;
end SimpleMemory;
		
package body SimpleMemory is

	function ConvertUInt32ToStdLogicVector(input: unsigned(31 downto 0)) return std_logic_vector is
	begin
		return std_logic_vector(input);
	end ConvertUInt32ToStdLogicVector;
	
	function ConvertStdLogicVectorToUInt32(input : std_logic_vector) return unsigned is
	begin
		return unsigned(input);
	end ConvertStdLogicVectorToUInt32;
	
	function ConvertBooleanToStdLogicVector(input: boolean) return std_logic_vector is 
	begin
		case input is
			when true => return X"FFFFFFFF";
			when false => return X"00000000";
			when others => return X"00000000";
		end case;
	end ConvertBooleanToStdLogicVector;

	function ConvertStdLogicVectorToBoolean(input : std_logic_vector) return boolean is 
	begin
		-- In .NET a false is all zeros while a true is at least one 1 bit (or more), so using the same logic here.
		return not(input = X"00000000");
	end ConvertStdLogicVectorToBoolean;

	function ConvertInt32ToStdLogicVector(input: signed(31 downto 0)) return std_logic_vector is
	begin
		return std_logic_vector(input);
	end ConvertInt32ToStdLogicVector;

	function ConvertStdLogicVectorToInt32(input : std_logic_vector) return signed is
	begin
		return signed(input);
	end ConvertStdLogicVectorToInt32;

	function ConvertCharToStdLogicVector(input: character) return std_logic_vector is
		variable characterIndex : integer;
	begin
		case input is
			when '0' => characterIndex :=  0;
			when '1' => characterIndex :=  1;
			when '2' => characterIndex :=  2;
			when '3' => characterIndex :=  3;
			when '4' => characterIndex :=  4;
			when '5' => characterIndex :=  5;
			when '6' => characterIndex :=  6;
			when '7' => characterIndex :=  7;
			when '8' => characterIndex :=  8;
			when '9' => characterIndex :=  9;
			when 'A' => characterIndex := 10;
			when 'B' => characterIndex := 11;
			when 'C' => characterIndex := 12;
			when 'D' => characterIndex := 13;
			when 'E' => characterIndex := 14;
			when 'F' => characterIndex := 15;
			when 'G' => characterIndex := 16;
			when 'H' => characterIndex := 17;
			when 'I' => characterIndex := 18;
			when 'J' => characterIndex := 19;
			when 'K' => characterIndex := 20;
			when 'L' => characterIndex := 21;
			when 'M' => characterIndex := 22;
			when 'N' => characterIndex := 23;
			when 'O' => characterIndex := 24;
			when 'P' => characterIndex := 25;
			when 'Q' => characterIndex := 26;
			when 'R' => characterIndex := 27;
			when 'S' => characterIndex := 28;
			when 'T' => characterIndex := 29;
			when 'U' => characterIndex := 30;
			when 'V' => characterIndex := 31;
			when 'W' => characterIndex := 32;
			when 'X' => characterIndex := 33;
			when 'Y' => characterIndex := 34;
			when 'Z' => characterIndex := 35;
			when others => characterIndex := 0;
		end case;
			
		return std_logic_vector(to_signed(characterIndex, 32));
	end ConvertCharToStdLogicVector;

	function ConvertStdLogicVectorToChar(input : std_logic_vector) return character is
		variable characterIndex     : integer;
	begin
		characterIndex := to_integer(unsigned(input));
		
		case characterIndex is
			when  0 => return '0';
			when  1 => return '1';
			when  2 => return '2';
			when  3 => return '3';
			when  4 => return '4';
			when  5 => return '5';
			when  6 => return '6';
			when  7 => return '7';
			when  8 => return '8';
			when  9 => return '9';
			when 10 => return 'A';
			when 11 => return 'B';
			when 12 => return 'C';
			when 13 => return 'D';
			when 14 => return 'E';
			when 15 => return 'F';
			when 16 => return 'G';
			when 17 => return 'H';
			when 18 => return 'I';
			when 19 => return 'J';
			when 20 => return 'K';
			when 21 => return 'L';
			when 22 => return 'M';
			when 23 => return 'N';
			when 24 => return 'O';
			when 25 => return 'P';
			when 26 => return 'Q';
			when 27 => return 'R';
			when 28 => return 'S';
			when 29 => return 'T';
			when 30 => return 'U';
			when 31 => return 'V';
			when 32 => return 'W';
			when 33 => return 'X';
			when 34 => return 'Y';
			when 35 => return 'Z';
			when others => return '?';
		end case;
	end ConvertStdLogicVectorToChar;

end SimpleMemory;
-- Hast_IP, logic generated from the input .NET assemblies starts here.
library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
library work;
use work.TypeConversion.all;
library work;
use work.SimpleMemory.all;

entity Hast_IP is 
    port(
        \DataIn\: In std_logic_vector(31 downto 0);
        \DataOut\: Out std_logic_vector(31 downto 0);
        \CellIndex\: Out integer;
        \ReadEnable\: Out boolean;
        \WriteEnable\: Out boolean;
        \ReadsDone\: In boolean;
        \WritesDone\: In boolean;
        \MemberId\: In integer;
        \Reset\: In std_logic;
        \Started\: In boolean;
        \Finished\: Out boolean;
        \Clock\: In std_logic
    );
    -- (Hast_IP ID removed for approval testing.)
    -- (Date and time removed for approval testing.)
    -- Generated by Hastlayer - hastlayer.com
end Hast_IP;

architecture Imp of Hast_IP is 
    -- This IP was generated by Hastlayer from .NET code to mimic the original logic. Note the following:
    -- * For each member (methods, functions) in .NET a state machine was generated. Each state machine's name corresponds to 
    --   the original member's name.
    -- * Inputs and outputs are passed between state machines as shared objects.
    -- * There are operations that take multiple clock cycles like interacting with the memory and long-running arithmetic operations 
    --   (modulo, division, multiplication). These are awaited in subsequent states but be aware that some states can take more 
    --   than one clock cycle to produce their output.
    -- * The ExternalInvocationProxy process dispatches invocations that were started from the outside to the state machines.
    -- * The InternalInvocationProxy processes dispatch invocations between state machines.

    -- Custom inter-dependent type declarations start
    type \unsigned32_Array\ is array (integer range <>) of unsigned(31 downto 0);
    type \Hast.Samples.Kpz.KpzKernels\ is record 
        \IsNull\: boolean;
        \gridRaw\: \unsigned32_Array\(0 to 63);
        \integerProbabilityP\: unsigned(31 downto 0);
        \integerProbabilityQ\: unsigned(31 downto 0);
        \TestMode\: boolean;
        \NumberOfIterations\: unsigned(31 downto 0);
        \randomState1\: unsigned(63 downto 0);
        \randomState2\: unsigned(63 downto 0);
    end record;
    -- Custom inter-dependent type declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations start
    -- State machine states:
    type \KpzKernelsInterface::DoIterations(SimpleMemory).0._States\ is (
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_0\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_1\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_2\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_3\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_6\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_7\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_8\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_9\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_10\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_11\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_12\, 
        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_13\);
    -- Signals:
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0._Finished\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Started.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Started.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).forceSwitch.parameter.Out.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Started.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Started.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0._Started\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Finished.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Finished.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Finished.0\: boolean := false;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Finished.0\: boolean := false;
    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::TestAdd(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations start
    -- State machine states:
    type \KpzKernelsInterface::TestAdd(SimpleMemory).0._States\ is (
        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_0\, 
        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_1\, 
        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_2\, 
        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_3\, 
        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_4\, 
        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_5\, 
        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_6\);
    -- Signals:
    Signal \KpzKernelsInterface::TestAdd(SimpleMemory).0._Finished\: boolean := false;
    Signal \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.CellIndex\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.DataOut\: std_logic_vector(31 downto 0);
    Signal \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\: boolean := false;
    Signal \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.WriteEnable\: boolean := false;
    Signal \KpzKernelsInterface::TestAdd(SimpleMemory).0._Started\: boolean := false;
    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::TestAdd(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations start
    -- State machine states:
    type \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._States\ is (
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_0\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_1\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_2\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_3\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_4\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_5\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_6\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_7\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_8\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_9\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_10\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_11\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_12\, 
        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_13\);
    -- Signals:
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Finished\: boolean := false;
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.DataOut\: std_logic_vector(31 downto 0);
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\: boolean := false;
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.WriteEnable\: boolean := false;
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Started\: boolean := false;
    Signal \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    -- System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations end


    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1().0 declarations start
    -- State machine states:
    type \KpzKernels::GetNextRandom1().0._States\ is (
        \KpzKernels::GetNextRandom1().0._State_0\, 
        \KpzKernels::GetNextRandom1().0._State_1\, 
        \KpzKernels::GetNextRandom1().0._State_2\);
    -- Signals:
    Signal \KpzKernels::GetNextRandom1().0._Finished\: boolean := false;
    Signal \KpzKernels::GetNextRandom1().0.return\: unsigned(31 downto 0) := to_unsigned(0, 32);
    Signal \KpzKernels::GetNextRandom1().0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::GetNextRandom1().0._Started\: boolean := false;
    Signal \KpzKernels::GetNextRandom1().0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1().0 declarations end


    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2().0 declarations start
    -- State machine states:
    type \KpzKernels::GetNextRandom2().0._States\ is (
        \KpzKernels::GetNextRandom2().0._State_0\, 
        \KpzKernels::GetNextRandom2().0._State_1\, 
        \KpzKernels::GetNextRandom2().0._State_2\);
    -- Signals:
    Signal \KpzKernels::GetNextRandom2().0._Finished\: boolean := false;
    Signal \KpzKernels::GetNextRandom2().0.return\: unsigned(31 downto 0) := to_unsigned(0, 32);
    Signal \KpzKernels::GetNextRandom2().0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::GetNextRandom2().0._Started\: boolean := false;
    Signal \KpzKernels::GetNextRandom2().0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2().0 declarations end


    -- System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32).0 declarations start
    -- State machine states:
    type \KpzKernels::getIndexFromXY(Int32,Int32).0._States\ is (
        \KpzKernels::getIndexFromXY(Int32,Int32).0._State_0\, 
        \KpzKernels::getIndexFromXY(Int32,Int32).0._State_1\, 
        \KpzKernels::getIndexFromXY(Int32,Int32).0._State_2\);
    -- Signals:
    Signal \KpzKernels::getIndexFromXY(Int32,Int32).0._Finished\: boolean := false;
    Signal \KpzKernels::getIndexFromXY(Int32,Int32).0.return\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::getIndexFromXY(Int32,Int32).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::getIndexFromXY(Int32,Int32).0._Started\: boolean := false;
    Signal \KpzKernels::getIndexFromXY(Int32,Int32).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::getIndexFromXY(Int32,Int32).0.x.parameter.In\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::getIndexFromXY(Int32,Int32).0.y.parameter.In\: signed(31 downto 0) := to_signed(0, 32);
    -- System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32).0 declarations end


    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32).0 declarations start
    -- State machine states:
    type \KpzKernels::getGridDx(Int32).0._States\ is (
        \KpzKernels::getGridDx(Int32).0._State_0\, 
        \KpzKernels::getGridDx(Int32).0._State_1\, 
        \KpzKernels::getGridDx(Int32).0._State_2\);
    -- Signals:
    Signal \KpzKernels::getGridDx(Int32).0._Finished\: boolean := false;
    Signal \KpzKernels::getGridDx(Int32).0.return\: boolean := false;
    Signal \KpzKernels::getGridDx(Int32).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::getGridDx(Int32).0._Started\: boolean := false;
    Signal \KpzKernels::getGridDx(Int32).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::getGridDx(Int32).0.index.parameter.In\: signed(31 downto 0) := to_signed(0, 32);
    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32).0 declarations end


    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32).0 declarations start
    -- State machine states:
    type \KpzKernels::getGridDy(Int32).0._States\ is (
        \KpzKernels::getGridDy(Int32).0._State_0\, 
        \KpzKernels::getGridDy(Int32).0._State_1\, 
        \KpzKernels::getGridDy(Int32).0._State_2\);
    -- Signals:
    Signal \KpzKernels::getGridDy(Int32).0._Finished\: boolean := false;
    Signal \KpzKernels::getGridDy(Int32).0.return\: boolean := false;
    Signal \KpzKernels::getGridDy(Int32).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::getGridDy(Int32).0._Started\: boolean := false;
    Signal \KpzKernels::getGridDy(Int32).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::getGridDy(Int32).0.index.parameter.In\: signed(31 downto 0) := to_signed(0, 32);
    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32).0 declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean).0 declarations start
    -- State machine states:
    type \KpzKernels::setGridDx(Int32,Boolean).0._States\ is (
        \KpzKernels::setGridDx(Int32,Boolean).0._State_0\, 
        \KpzKernels::setGridDx(Int32,Boolean).0._State_1\, 
        \KpzKernels::setGridDx(Int32,Boolean).0._State_2\, 
        \KpzKernels::setGridDx(Int32,Boolean).0._State_3\, 
        \KpzKernels::setGridDx(Int32,Boolean).0._State_4\, 
        \KpzKernels::setGridDx(Int32,Boolean).0._State_5\);
    -- Signals:
    Signal \KpzKernels::setGridDx(Int32,Boolean).0._Finished\: boolean := false;
    Signal \KpzKernels::setGridDx(Int32,Boolean).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::setGridDx(Int32,Boolean).0._Started\: boolean := false;
    Signal \KpzKernels::setGridDx(Int32,Boolean).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::setGridDx(Int32,Boolean).0.index.parameter.In\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::setGridDx(Int32,Boolean).0.value.parameter.In\: boolean := false;
    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean).0 declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean).0 declarations start
    -- State machine states:
    type \KpzKernels::setGridDy(Int32,Boolean).0._States\ is (
        \KpzKernels::setGridDy(Int32,Boolean).0._State_0\, 
        \KpzKernels::setGridDy(Int32,Boolean).0._State_1\, 
        \KpzKernels::setGridDy(Int32,Boolean).0._State_2\, 
        \KpzKernels::setGridDy(Int32,Boolean).0._State_3\, 
        \KpzKernels::setGridDy(Int32,Boolean).0._State_4\, 
        \KpzKernels::setGridDy(Int32,Boolean).0._State_5\);
    -- Signals:
    Signal \KpzKernels::setGridDy(Int32,Boolean).0._Finished\: boolean := false;
    Signal \KpzKernels::setGridDy(Int32,Boolean).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::setGridDy(Int32,Boolean).0._Started\: boolean := false;
    Signal \KpzKernels::setGridDy(Int32,Boolean).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::setGridDy(Int32,Boolean).0.index.parameter.In\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::setGridDy(Int32,Boolean).0.value.parameter.In\: boolean := false;
    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean).0 declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations start
    -- State machine states:
    type \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._States\ is (
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_0\, 
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_1\, 
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_2\, 
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_3\, 
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_4\, 
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_5\, 
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_6\, 
        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_7\);
    -- Signals:
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Finished\: boolean := false;
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.DataOut\: std_logic_vector(31 downto 0);
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\: boolean := false;
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\: boolean := false;
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Started\: boolean := false;
    Signal \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations start
    -- State machine states:
    type \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._States\ is (
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_0\, 
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_1\, 
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_2\, 
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_3\, 
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_4\, 
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_5\, 
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_6\, 
        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_7\);
    -- Signals:
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Finished\: boolean := false;
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.DataOut\: std_logic_vector(31 downto 0);
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\: boolean := false;
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\: boolean := false;
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Started\: boolean := false;
    Signal \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 declarations end


    -- System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 declarations start
    -- State machine states:
    type \KpzKernels::RandomlySwitchFourCells(Boolean).0._States\ is (
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_0\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_1\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_2\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_3\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_4\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_7\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_8\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_9\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_10\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_11\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_12\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_13\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_14\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_15\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_16\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_17\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_18\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_19\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_20\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_21\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_22\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_23\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_24\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_25\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_26\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_27\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_28\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_29\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_30\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_31\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_32\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_33\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_34\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_35\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_36\, 
        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_37\);
    -- Signals:
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0._Finished\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.this.parameter.Out\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Started.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).x.parameter.Out.0\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).y.parameter.Out.0\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Started.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Started.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).index.parameter.Out.0\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).value.parameter.Out.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.Out.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).index.parameter.Out.0\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).value.parameter.Out.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0._Started\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.this.parameter.In\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch.parameter.In\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Finished.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().return.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Finished.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).return.0\: signed(31 downto 0) := to_signed(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Finished.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().return.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Finished.0\: boolean := false;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.In.0\: \Hast.Samples.Kpz.KpzKernels\;
    Signal \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Finished.0\: boolean := false;
    -- System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 declarations end


    -- System.Void Hast::ExternalInvocationProxy() declarations start
    -- Signals:
    Signal \FinishedInternal\: boolean := false;
    Signal \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Started.0\: boolean := false;
    Signal \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Started.0\: boolean := false;
    Signal \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Finished.0\: boolean := false;
    Signal \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Finished.0\: boolean := false;
    -- System.Void Hast::ExternalInvocationProxy() declarations end


    -- \System.Void Hast::InternalInvocationProxy()._CommonDeclarations\ declarations start
    type \InternalInvocationProxy_boolean_Array\ is array (integer range <>) of boolean;
    type \Hast::InternalInvocationProxy()._RunningStates\ is (
        WaitingForStarted, 
        WaitingForFinished, 
        AfterFinished);
    -- \System.Void Hast::InternalInvocationProxy()._CommonDeclarations\ declarations end

begin 

    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine start
    \KpzKernelsInterface::DoIterations(SimpleMemory).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\: \KpzKernelsInterface::DoIterations(SimpleMemory).0._States\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_0\;
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.num\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.num2\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.i\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.0\: boolean := false;
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.1\: boolean := false;
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.2\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.3\: signed(31 downto 0) := to_signed(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernelsInterface::DoIterations(SimpleMemory).0._Finished\ <= false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Started.0\ <= false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Started.0\ <= false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).forceSwitch.parameter.Out.0\ <= false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Started.0\ <= false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Started.0\ <= false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_0\;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.num\ := to_signed(0, 32);
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.num2\ := to_signed(0, 32);
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.i\ := to_signed(0, 32);
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.0\ := false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.1\ := false;
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.2\ := to_signed(0, 32);
                \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.3\ := to_signed(0, 32);
            else 
                case \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ is 
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0._Started\ = true) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0._Started\ = true) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._Finished\ <= true;
                        else 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._Finished\ <= false;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_0\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_2\ => 
                        -- Initializing record fields to their defaults.
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\IsNull\ := false;
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\gridRaw\ := (others => to_unsigned(0, 32));
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\integerProbabilityP\ := to_unsigned(32767, 32);
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\integerProbabilityQ\ := to_unsigned(32767, 32);
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\TestMode\ := False;
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\NumberOfIterations\ := to_unsigned(1, 32);
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\randomState1\ := to_unsigned(0, 64);
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\randomState2\ := to_unsigned(0, 64);
                        -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory)
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).this.parameter.Out.0\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\;
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Started.0\ <= true;
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_3\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_3\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory)
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Started.0\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Finished.0\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Started.0\ <= false;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory)
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory).this.parameter.Out.0\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Started.0\ <= true;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory)
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Started.0\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Finished.0\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Started.0\ <= false;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory).this.parameter.In.0\;

                            -- This if-else was transformed from a .NET if-else. It spans across multiple states:
                            --     * The true branch starts in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_6\ and ends in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_6\.
                            --     * The false branch starts in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_7\ and ends in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_7\.
                            --     * Execution after either branch will continue in the following state: \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\.

                            if (\KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\TestMode\) then 
                                \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_6\;
                            else 
                                \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_7\;
                            end if;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\ => 
                        -- State after the if-else which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\.
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.num2\ := to_signed(0, 32);
                        -- Starting a while loop.
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_8\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_6\ => 
                        -- True branch of the if-else started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\.
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.num\ := to_signed(1, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\.
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_6\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_7\ => 
                        -- False branch of the if-else started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\.
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.num\ := to_signed(64, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_4\.
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_7\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_8\ => 
                        -- Repeated state of the while loop which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\.
                        -- The while loop's condition:
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.0\ := SmartResize(\KpzKernelsInterface::DoIterations(SimpleMemory).0.num2\, 64) < signed((SmartResize(\KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\NumberOfIterations\, 64)));
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.0\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.i\ := to_signed(0, 32);
                            -- Starting a while loop.
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_10\;
                        else 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_9\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_9\ => 
                        -- State after the while loop which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\.
                        -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory)
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).this.parameter.Out.0\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\;
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Started.0\ <= true;
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_13\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_10\ => 
                        -- Repeated state of the while loop which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_8\.
                        -- The while loop's condition:
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.1\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.i\ < \KpzKernelsInterface::DoIterations(SimpleMemory).0.num\;
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.1\) then 
                            -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean)
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).this.parameter.Out.0\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).forceSwitch.parameter.Out.0\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\.\TestMode\;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Started.0\ <= true;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_12\;
                        else 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_11\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_11\ => 
                        -- State after the while loop which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_8\.
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.3\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.num2\ + to_signed(1, 32);
                        \KpzKernelsInterface::DoIterations(SimpleMemory).0.num2\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.3\;
                        -- Returning to the repeated state of the while loop which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_5\ if the loop wasn't exited with a state change.
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_11\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_8\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_12\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean)
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Started.0\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Finished.0\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Started.0\ <= false;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).this.parameter.In.0\;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.2\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.i\ + to_signed(1, 32);
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.i\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.binaryOperationResult.2\;
                            -- Returning to the repeated state of the while loop which was started in state \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_8\ if the loop wasn't exited with a state change.
                            if (\KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_12\) then 
                                \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_10\;
                            end if;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_13\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory)
                        if (\KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Started.0\ = \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Finished.0\) then 
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Started.0\ <= false;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0.kpzKernels\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).this.parameter.In.0\;
                            \KpzKernelsInterface::DoIterations(SimpleMemory).0._State\ := \KpzKernelsInterface::DoIterations(SimpleMemory).0._State_1\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine end


    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::TestAdd(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine start
    \KpzKernelsInterface::TestAdd(SimpleMemory).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\: \KpzKernelsInterface::TestAdd(SimpleMemory).0._States\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_0\;
        Variable \KpzKernelsInterface::TestAdd(SimpleMemory).0.dataIn.0\: std_logic_vector(31 downto 0);
        Variable \KpzKernelsInterface::TestAdd(SimpleMemory).0.dataIn.1\: std_logic_vector(31 downto 0);
        Variable \KpzKernelsInterface::TestAdd(SimpleMemory).0.binaryOperationResult.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernelsInterface::TestAdd(SimpleMemory).0._Finished\ <= false;
                \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.CellIndex\ <= to_signed(0, 32);
                \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.WriteEnable\ <= false;
                \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_0\;
                \KpzKernelsInterface::TestAdd(SimpleMemory).0.binaryOperationResult.0\ := to_unsigned(0, 32);
            else 
                case \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ is 
                    when \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernelsInterface::TestAdd(SimpleMemory).0._Started\ = true) then 
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernelsInterface::TestAdd(SimpleMemory).0._Started\ = true) then 
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0._Finished\ <= true;
                        else 
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0._Finished\ <= false;
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_0\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_2\ => 
                        -- Begin SimpleMemory read.
                        \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(0, 32), 32);
                        \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_3\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_3\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.dataIn.0\ := \DataIn\;
                            -- The last SimpleMemory read just finished, so need to start the next one in the next state.
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_4\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_4\ => 
                        -- Begin SimpleMemory read.
                        \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(1, 32), 32);
                        \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_5\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_5\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.dataIn.1\ := \DataIn\;
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.binaryOperationResult.0\ := ConvertStdLogicVectorToUInt32(\KpzKernelsInterface::TestAdd(SimpleMemory).0.dataIn.0\) + ConvertStdLogicVectorToUInt32(\KpzKernelsInterface::TestAdd(SimpleMemory).0.dataIn.1\);
                            -- Begin SimpleMemory write.
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(2, 32), 32);
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.WriteEnable\ <= true;
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.DataOut\ <= ConvertUInt32ToStdLogicVector(\KpzKernelsInterface::TestAdd(SimpleMemory).0.binaryOperationResult.0\);
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_6\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_6\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\WritesDone\ = true) then 
                            -- SimpleMemory write finished.
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.WriteEnable\ <= false;
                            \KpzKernelsInterface::TestAdd(SimpleMemory).0._State\ := \KpzKernelsInterface::TestAdd(SimpleMemory).0._State_1\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernelsInterface::TestAdd(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine end


    -- System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine start
    \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\: \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._States\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_0\;
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.0\: std_logic_vector(31 downto 0);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.0\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.1\: std_logic_vector(31 downto 0);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.1\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.2\: std_logic_vector(31 downto 0);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.2\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.3\: std_logic_vector(31 downto 0);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.3\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.4\: std_logic_vector(31 downto 0);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.4\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.5\: boolean := false;
        Variable \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.5\: std_logic_vector(31 downto 0);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Finished\ <= false;
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\ <= to_signed(0, 32);
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.WriteEnable\ <= false;
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_0\;
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.0\ := to_unsigned(0, 64);
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.1\ := to_unsigned(0, 64);
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.2\ := to_unsigned(0, 64);
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.3\ := to_unsigned(0, 64);
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.4\ := to_unsigned(0, 32);
                \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.5\ := false;
            else 
                case \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ is 
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Started\ = true) then 
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Started\ = true) then 
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Finished\ <= true;
                        else 
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Finished\ <= false;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this.parameter.Out\ <= \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_2\ => 
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this.parameter.In\;
                        -- Begin SimpleMemory read.
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(64, 32), 32);
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_3\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_3\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.0\ := \DataIn\;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.0\ := SmartResize(shift_left(SmartResize(ConvertStdLogicVectorToUInt32(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.0\), 64), to_integer(to_signed(32, 32))), 64);
                            -- The last SimpleMemory read just finished, so need to start the next one in the next state.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_4\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_4\ => 
                        -- Begin SimpleMemory read.
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(65, 32), 32);
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_5\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_5\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.1\ := \DataIn\;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.1\ := resize(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.0\ or SmartResize(ConvertStdLogicVectorToUInt32(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.1\), 64), 64);
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this\.\randomState1\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.1\;
                            -- The last SimpleMemory read just finished, so need to start the next one in the next state.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_6\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_6\ => 
                        -- Begin SimpleMemory read.
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(66, 32), 32);
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_7\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_7\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.2\ := \DataIn\;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.2\ := SmartResize(shift_left(SmartResize(ConvertStdLogicVectorToUInt32(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.2\), 64), to_integer(to_signed(32, 32))), 64);
                            -- The last SimpleMemory read just finished, so need to start the next one in the next state.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_8\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_8\ => 
                        -- Begin SimpleMemory read.
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(67, 32), 32);
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_9\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_9\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.3\ := \DataIn\;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.3\ := resize(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.2\ or SmartResize(ConvertStdLogicVectorToUInt32(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.3\), 64), 64);
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this\.\randomState2\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.3\;
                            -- The last SimpleMemory read just finished, so need to start the next one in the next state.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_10\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_10\ => 
                        -- Begin SimpleMemory read.
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(68, 32), 32);
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_11\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_11\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.4\ := \DataIn\;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.4\ := ConvertStdLogicVectorToUInt32(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.4\) and to_unsigned(1, 32);
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.5\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.4\ = to_unsigned(1, 32);
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this\.\TestMode\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.binaryOperationResult.5\;
                            -- The last SimpleMemory read just finished, so need to start the next one in the next state.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_12\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_12\ => 
                        -- Begin SimpleMemory read.
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(to_signed(69, 32), 32);
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                        \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_13\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_13\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.5\ := \DataIn\;
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this\.\NumberOfIterations\ := ConvertStdLogicVectorToUInt32(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.dataIn.5\);
                            \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State\ := \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._State_1\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine end


    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1().0 state machine start
    \KpzKernels::GetNextRandom1().0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::GetNextRandom1().0._State\: \KpzKernels::GetNextRandom1().0._States\ := \KpzKernels::GetNextRandom1().0._State_0\;
        Variable \KpzKernels::GetNextRandom1().0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::GetNextRandom1().0.num\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom1().0.num2\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom1().0.binaryOperationResult.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom1().0.binaryOperationResult.1\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom1().0.binaryOperationResult.2\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::GetNextRandom1().0.binaryOperationResult.3\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::GetNextRandom1().0.binaryOperationResult.4\: unsigned(31 downto 0) := to_unsigned(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::GetNextRandom1().0._Finished\ <= false;
                \KpzKernels::GetNextRandom1().0.return\ <= to_unsigned(0, 32);
                \KpzKernels::GetNextRandom1().0._State\ := \KpzKernels::GetNextRandom1().0._State_0\;
                \KpzKernels::GetNextRandom1().0.num\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom1().0.num2\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom1().0.binaryOperationResult.0\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom1().0.binaryOperationResult.1\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom1().0.binaryOperationResult.2\ := to_unsigned(0, 64);
                \KpzKernels::GetNextRandom1().0.binaryOperationResult.3\ := to_unsigned(0, 64);
                \KpzKernels::GetNextRandom1().0.binaryOperationResult.4\ := to_unsigned(0, 32);
            else 
                case \KpzKernels::GetNextRandom1().0._State\ is 
                    when \KpzKernels::GetNextRandom1().0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::GetNextRandom1().0._Started\ = true) then 
                            \KpzKernels::GetNextRandom1().0._State\ := \KpzKernels::GetNextRandom1().0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::GetNextRandom1().0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::GetNextRandom1().0._Started\ = true) then 
                            \KpzKernels::GetNextRandom1().0._Finished\ <= true;
                        else 
                            \KpzKernels::GetNextRandom1().0._Finished\ <= false;
                            \KpzKernels::GetNextRandom1().0._State\ := \KpzKernels::GetNextRandom1().0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::GetNextRandom1().0.this.parameter.Out\ <= \KpzKernels::GetNextRandom1().0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::GetNextRandom1().0._State_2\ => 
                        \KpzKernels::GetNextRandom1().0.this\ := \KpzKernels::GetNextRandom1().0.this.parameter.In\;
                        \KpzKernels::GetNextRandom1().0.binaryOperationResult.0\ := SmartResize(shift_right(\KpzKernels::GetNextRandom1().0.this\.\randomState1\, to_integer(to_signed(32, 32))), 32);
                        \KpzKernels::GetNextRandom1().0.num\ := (\KpzKernels::GetNextRandom1().0.binaryOperationResult.0\);
                        -- Since the integer literal 18446744073709551615 was out of the VHDL integer range it was substituted with a binary literal (1111111111111111111111111111111111111111111111111111111111111111).
                        \KpzKernels::GetNextRandom1().0.binaryOperationResult.1\ := SmartResize(\KpzKernels::GetNextRandom1().0.this\.\randomState1\ and "1111111111111111111111111111111111111111111111111111111111111111", 32);
                        \KpzKernels::GetNextRandom1().0.num2\ := (\KpzKernels::GetNextRandom1().0.binaryOperationResult.1\);
                        -- Since the integer literal 18446744073709467675 was out of the VHDL integer range it was substituted with a binary literal (1111111111111111111111111111111111111111111111101011100000011011).
                        \KpzKernels::GetNextRandom1().0.binaryOperationResult.2\ := resize(SmartResize(\KpzKernels::GetNextRandom1().0.num2\, 64) * "1111111111111111111111111111111111111111111111101011100000011011", 64);
                        \KpzKernels::GetNextRandom1().0.binaryOperationResult.3\ := resize(\KpzKernels::GetNextRandom1().0.binaryOperationResult.2\ + SmartResize(\KpzKernels::GetNextRandom1().0.num\, 64), 64);
                        \KpzKernels::GetNextRandom1().0.this\.\randomState1\ := \KpzKernels::GetNextRandom1().0.binaryOperationResult.3\;
                        \KpzKernels::GetNextRandom1().0.binaryOperationResult.4\ := \KpzKernels::GetNextRandom1().0.num2\ xor \KpzKernels::GetNextRandom1().0.num\;
                        \KpzKernels::GetNextRandom1().0.return\ <= \KpzKernels::GetNextRandom1().0.binaryOperationResult.4\;
                        \KpzKernels::GetNextRandom1().0._State\ := \KpzKernels::GetNextRandom1().0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0.5
                end case;
            end if;
        end if;
    end process;
    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1().0 state machine end


    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2().0 state machine start
    \KpzKernels::GetNextRandom2().0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::GetNextRandom2().0._State\: \KpzKernels::GetNextRandom2().0._States\ := \KpzKernels::GetNextRandom2().0._State_0\;
        Variable \KpzKernels::GetNextRandom2().0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::GetNextRandom2().0.num\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom2().0.num2\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom2().0.binaryOperationResult.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom2().0.binaryOperationResult.1\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::GetNextRandom2().0.binaryOperationResult.2\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::GetNextRandom2().0.binaryOperationResult.3\: unsigned(63 downto 0) := to_unsigned(0, 64);
        Variable \KpzKernels::GetNextRandom2().0.binaryOperationResult.4\: unsigned(31 downto 0) := to_unsigned(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::GetNextRandom2().0._Finished\ <= false;
                \KpzKernels::GetNextRandom2().0.return\ <= to_unsigned(0, 32);
                \KpzKernels::GetNextRandom2().0._State\ := \KpzKernels::GetNextRandom2().0._State_0\;
                \KpzKernels::GetNextRandom2().0.num\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom2().0.num2\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom2().0.binaryOperationResult.0\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom2().0.binaryOperationResult.1\ := to_unsigned(0, 32);
                \KpzKernels::GetNextRandom2().0.binaryOperationResult.2\ := to_unsigned(0, 64);
                \KpzKernels::GetNextRandom2().0.binaryOperationResult.3\ := to_unsigned(0, 64);
                \KpzKernels::GetNextRandom2().0.binaryOperationResult.4\ := to_unsigned(0, 32);
            else 
                case \KpzKernels::GetNextRandom2().0._State\ is 
                    when \KpzKernels::GetNextRandom2().0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::GetNextRandom2().0._Started\ = true) then 
                            \KpzKernels::GetNextRandom2().0._State\ := \KpzKernels::GetNextRandom2().0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::GetNextRandom2().0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::GetNextRandom2().0._Started\ = true) then 
                            \KpzKernels::GetNextRandom2().0._Finished\ <= true;
                        else 
                            \KpzKernels::GetNextRandom2().0._Finished\ <= false;
                            \KpzKernels::GetNextRandom2().0._State\ := \KpzKernels::GetNextRandom2().0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::GetNextRandom2().0.this.parameter.Out\ <= \KpzKernels::GetNextRandom2().0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::GetNextRandom2().0._State_2\ => 
                        \KpzKernels::GetNextRandom2().0.this\ := \KpzKernels::GetNextRandom2().0.this.parameter.In\;
                        \KpzKernels::GetNextRandom2().0.binaryOperationResult.0\ := SmartResize(shift_right(\KpzKernels::GetNextRandom2().0.this\.\randomState2\, to_integer(to_signed(32, 32))), 32);
                        \KpzKernels::GetNextRandom2().0.num\ := (\KpzKernels::GetNextRandom2().0.binaryOperationResult.0\);
                        -- Since the integer literal 18446744073709551615 was out of the VHDL integer range it was substituted with a binary literal (1111111111111111111111111111111111111111111111111111111111111111).
                        \KpzKernels::GetNextRandom2().0.binaryOperationResult.1\ := SmartResize(\KpzKernels::GetNextRandom2().0.this\.\randomState2\ and "1111111111111111111111111111111111111111111111111111111111111111", 32);
                        \KpzKernels::GetNextRandom2().0.num2\ := (\KpzKernels::GetNextRandom2().0.binaryOperationResult.1\);
                        -- Since the integer literal 18446744073709467675 was out of the VHDL integer range it was substituted with a binary literal (1111111111111111111111111111111111111111111111101011100000011011).
                        \KpzKernels::GetNextRandom2().0.binaryOperationResult.2\ := resize(SmartResize(\KpzKernels::GetNextRandom2().0.num2\, 64) * "1111111111111111111111111111111111111111111111101011100000011011", 64);
                        \KpzKernels::GetNextRandom2().0.binaryOperationResult.3\ := resize(\KpzKernels::GetNextRandom2().0.binaryOperationResult.2\ + SmartResize(\KpzKernels::GetNextRandom2().0.num\, 64), 64);
                        \KpzKernels::GetNextRandom2().0.this\.\randomState2\ := \KpzKernels::GetNextRandom2().0.binaryOperationResult.3\;
                        \KpzKernels::GetNextRandom2().0.binaryOperationResult.4\ := \KpzKernels::GetNextRandom2().0.num2\ xor \KpzKernels::GetNextRandom2().0.num\;
                        \KpzKernels::GetNextRandom2().0.return\ <= \KpzKernels::GetNextRandom2().0.binaryOperationResult.4\;
                        \KpzKernels::GetNextRandom2().0._State\ := \KpzKernels::GetNextRandom2().0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0.5
                end case;
            end if;
        end if;
    end process;
    -- System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2().0 state machine end


    -- System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32).0 state machine start
    \KpzKernels::getIndexFromXY(Int32,Int32).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::getIndexFromXY(Int32,Int32).0._State\: \KpzKernels::getIndexFromXY(Int32,Int32).0._States\ := \KpzKernels::getIndexFromXY(Int32,Int32).0._State_0\;
        Variable \KpzKernels::getIndexFromXY(Int32,Int32).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::getIndexFromXY(Int32,Int32).0.x\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::getIndexFromXY(Int32,Int32).0.y\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.0\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.1\: signed(31 downto 0) := to_signed(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::getIndexFromXY(Int32,Int32).0._Finished\ <= false;
                \KpzKernels::getIndexFromXY(Int32,Int32).0.return\ <= to_signed(0, 32);
                \KpzKernels::getIndexFromXY(Int32,Int32).0._State\ := \KpzKernels::getIndexFromXY(Int32,Int32).0._State_0\;
                \KpzKernels::getIndexFromXY(Int32,Int32).0.x\ := to_signed(0, 32);
                \KpzKernels::getIndexFromXY(Int32,Int32).0.y\ := to_signed(0, 32);
                \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.0\ := to_signed(0, 32);
                \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.1\ := to_signed(0, 32);
            else 
                case \KpzKernels::getIndexFromXY(Int32,Int32).0._State\ is 
                    when \KpzKernels::getIndexFromXY(Int32,Int32).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::getIndexFromXY(Int32,Int32).0._Started\ = true) then 
                            \KpzKernels::getIndexFromXY(Int32,Int32).0._State\ := \KpzKernels::getIndexFromXY(Int32,Int32).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::getIndexFromXY(Int32,Int32).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::getIndexFromXY(Int32,Int32).0._Started\ = true) then 
                            \KpzKernels::getIndexFromXY(Int32,Int32).0._Finished\ <= true;
                        else 
                            \KpzKernels::getIndexFromXY(Int32,Int32).0._Finished\ <= false;
                            \KpzKernels::getIndexFromXY(Int32,Int32).0._State\ := \KpzKernels::getIndexFromXY(Int32,Int32).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::getIndexFromXY(Int32,Int32).0.this.parameter.Out\ <= \KpzKernels::getIndexFromXY(Int32,Int32).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::getIndexFromXY(Int32,Int32).0._State_2\ => 
                        \KpzKernels::getIndexFromXY(Int32,Int32).0.this\ := \KpzKernels::getIndexFromXY(Int32,Int32).0.this.parameter.In\;
                        \KpzKernels::getIndexFromXY(Int32,Int32).0.x\ := \KpzKernels::getIndexFromXY(Int32,Int32).0.x.parameter.In\;
                        \KpzKernels::getIndexFromXY(Int32,Int32).0.y\ := \KpzKernels::getIndexFromXY(Int32,Int32).0.y.parameter.In\;
                        \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.0\ := resize(\KpzKernels::getIndexFromXY(Int32,Int32).0.y\ * to_signed(8, 32), 32);
                        \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.1\ := \KpzKernels::getIndexFromXY(Int32,Int32).0.x\ + \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.0\;
                        \KpzKernels::getIndexFromXY(Int32,Int32).0.return\ <= \KpzKernels::getIndexFromXY(Int32,Int32).0.binaryOperationResult.1\;
                        \KpzKernels::getIndexFromXY(Int32,Int32).0._State\ := \KpzKernels::getIndexFromXY(Int32,Int32).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                end case;
            end if;
        end if;
    end process;
    -- System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32).0 state machine end


    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32).0 state machine start
    \KpzKernels::getGridDx(Int32).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::getGridDx(Int32).0._State\: \KpzKernels::getGridDx(Int32).0._States\ := \KpzKernels::getGridDx(Int32).0._State_0\;
        Variable \KpzKernels::getGridDx(Int32).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::getGridDx(Int32).0.index\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::getGridDx(Int32).0.binaryOperationResult.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::getGridDx(Int32).0.binaryOperationResult.1\: boolean := false;
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::getGridDx(Int32).0._Finished\ <= false;
                \KpzKernels::getGridDx(Int32).0.return\ <= false;
                \KpzKernels::getGridDx(Int32).0._State\ := \KpzKernels::getGridDx(Int32).0._State_0\;
                \KpzKernels::getGridDx(Int32).0.index\ := to_signed(0, 32);
                \KpzKernels::getGridDx(Int32).0.binaryOperationResult.0\ := to_unsigned(0, 32);
                \KpzKernels::getGridDx(Int32).0.binaryOperationResult.1\ := false;
            else 
                case \KpzKernels::getGridDx(Int32).0._State\ is 
                    when \KpzKernels::getGridDx(Int32).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::getGridDx(Int32).0._Started\ = true) then 
                            \KpzKernels::getGridDx(Int32).0._State\ := \KpzKernels::getGridDx(Int32).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::getGridDx(Int32).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::getGridDx(Int32).0._Started\ = true) then 
                            \KpzKernels::getGridDx(Int32).0._Finished\ <= true;
                        else 
                            \KpzKernels::getGridDx(Int32).0._Finished\ <= false;
                            \KpzKernels::getGridDx(Int32).0._State\ := \KpzKernels::getGridDx(Int32).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::getGridDx(Int32).0.this.parameter.Out\ <= \KpzKernels::getGridDx(Int32).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::getGridDx(Int32).0._State_2\ => 
                        \KpzKernels::getGridDx(Int32).0.this\ := \KpzKernels::getGridDx(Int32).0.this.parameter.In\;
                        \KpzKernels::getGridDx(Int32).0.index\ := \KpzKernels::getGridDx(Int32).0.index.parameter.In\;
                        \KpzKernels::getGridDx(Int32).0.binaryOperationResult.0\ := \KpzKernels::getGridDx(Int32).0.this\.\gridRaw\(to_integer(\KpzKernels::getGridDx(Int32).0.index\)) and to_unsigned(1, 32);
                        \KpzKernels::getGridDx(Int32).0.binaryOperationResult.1\ := \KpzKernels::getGridDx(Int32).0.binaryOperationResult.0\ > to_unsigned(0, 32);
                        \KpzKernels::getGridDx(Int32).0.return\ <= \KpzKernels::getGridDx(Int32).0.binaryOperationResult.1\;
                        \KpzKernels::getGridDx(Int32).0._State\ := \KpzKernels::getGridDx(Int32).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                end case;
            end if;
        end if;
    end process;
    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32).0 state machine end


    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32).0 state machine start
    \KpzKernels::getGridDy(Int32).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::getGridDy(Int32).0._State\: \KpzKernels::getGridDy(Int32).0._States\ := \KpzKernels::getGridDy(Int32).0._State_0\;
        Variable \KpzKernels::getGridDy(Int32).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::getGridDy(Int32).0.index\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::getGridDy(Int32).0.binaryOperationResult.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::getGridDy(Int32).0.binaryOperationResult.1\: boolean := false;
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::getGridDy(Int32).0._Finished\ <= false;
                \KpzKernels::getGridDy(Int32).0.return\ <= false;
                \KpzKernels::getGridDy(Int32).0._State\ := \KpzKernels::getGridDy(Int32).0._State_0\;
                \KpzKernels::getGridDy(Int32).0.index\ := to_signed(0, 32);
                \KpzKernels::getGridDy(Int32).0.binaryOperationResult.0\ := to_unsigned(0, 32);
                \KpzKernels::getGridDy(Int32).0.binaryOperationResult.1\ := false;
            else 
                case \KpzKernels::getGridDy(Int32).0._State\ is 
                    when \KpzKernels::getGridDy(Int32).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::getGridDy(Int32).0._Started\ = true) then 
                            \KpzKernels::getGridDy(Int32).0._State\ := \KpzKernels::getGridDy(Int32).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::getGridDy(Int32).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::getGridDy(Int32).0._Started\ = true) then 
                            \KpzKernels::getGridDy(Int32).0._Finished\ <= true;
                        else 
                            \KpzKernels::getGridDy(Int32).0._Finished\ <= false;
                            \KpzKernels::getGridDy(Int32).0._State\ := \KpzKernels::getGridDy(Int32).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::getGridDy(Int32).0.this.parameter.Out\ <= \KpzKernels::getGridDy(Int32).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::getGridDy(Int32).0._State_2\ => 
                        \KpzKernels::getGridDy(Int32).0.this\ := \KpzKernels::getGridDy(Int32).0.this.parameter.In\;
                        \KpzKernels::getGridDy(Int32).0.index\ := \KpzKernels::getGridDy(Int32).0.index.parameter.In\;
                        \KpzKernels::getGridDy(Int32).0.binaryOperationResult.0\ := \KpzKernels::getGridDy(Int32).0.this\.\gridRaw\(to_integer(\KpzKernels::getGridDy(Int32).0.index\)) and to_unsigned(2, 32);
                        \KpzKernels::getGridDy(Int32).0.binaryOperationResult.1\ := \KpzKernels::getGridDy(Int32).0.binaryOperationResult.0\ > to_unsigned(0, 32);
                        \KpzKernels::getGridDy(Int32).0.return\ <= \KpzKernels::getGridDy(Int32).0.binaryOperationResult.1\;
                        \KpzKernels::getGridDy(Int32).0._State\ := \KpzKernels::getGridDy(Int32).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                end case;
            end if;
        end if;
    end process;
    -- System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32).0 state machine end


    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean).0 state machine start
    \KpzKernels::setGridDx(Int32,Boolean).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::setGridDx(Int32,Boolean).0._State\: \KpzKernels::setGridDx(Int32,Boolean).0._States\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_0\;
        Variable \KpzKernels::setGridDx(Int32,Boolean).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::setGridDx(Int32,Boolean).0.index\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::setGridDx(Int32,Boolean).0.value\: boolean := false;
        Variable \KpzKernels::setGridDx(Int32,Boolean).0.conditional8967f26a659a1ddcbabf9fc3ff6f8f89ca99389e2d9f85e3bbeaa263145b233e\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.1\: unsigned(31 downto 0) := to_unsigned(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::setGridDx(Int32,Boolean).0._Finished\ <= false;
                \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_0\;
                \KpzKernels::setGridDx(Int32,Boolean).0.index\ := to_signed(0, 32);
                \KpzKernels::setGridDx(Int32,Boolean).0.value\ := false;
                \KpzKernels::setGridDx(Int32,Boolean).0.conditional8967f26a659a1ddcbabf9fc3ff6f8f89ca99389e2d9f85e3bbeaa263145b233e\ := to_unsigned(0, 32);
                \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.0\ := to_unsigned(0, 32);
                \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.1\ := to_unsigned(0, 32);
            else 
                case \KpzKernels::setGridDx(Int32,Boolean).0._State\ is 
                    when \KpzKernels::setGridDx(Int32,Boolean).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::setGridDx(Int32,Boolean).0._Started\ = true) then 
                            \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDx(Int32,Boolean).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::setGridDx(Int32,Boolean).0._Started\ = true) then 
                            \KpzKernels::setGridDx(Int32,Boolean).0._Finished\ <= true;
                        else 
                            \KpzKernels::setGridDx(Int32,Boolean).0._Finished\ <= false;
                            \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::setGridDx(Int32,Boolean).0.this.parameter.Out\ <= \KpzKernels::setGridDx(Int32,Boolean).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDx(Int32,Boolean).0._State_2\ => 
                        \KpzKernels::setGridDx(Int32,Boolean).0.this\ := \KpzKernels::setGridDx(Int32,Boolean).0.this.parameter.In\;
                        \KpzKernels::setGridDx(Int32,Boolean).0.index\ := \KpzKernels::setGridDx(Int32,Boolean).0.index.parameter.In\;
                        \KpzKernels::setGridDx(Int32,Boolean).0.value\ := \KpzKernels::setGridDx(Int32,Boolean).0.value.parameter.In\;

                        -- This if-else was transformed from a .NET if-else. It spans across multiple states:
                        --     * The true branch starts in state \KpzKernels::setGridDx(Int32,Boolean).0._State_4\ and ends in state \KpzKernels::setGridDx(Int32,Boolean).0._State_4\.
                        --     * The false branch starts in state \KpzKernels::setGridDx(Int32,Boolean).0._State_5\ and ends in state \KpzKernels::setGridDx(Int32,Boolean).0._State_5\.
                        --     * Execution after either branch will continue in the following state: \KpzKernels::setGridDx(Int32,Boolean).0._State_3\.

                        if (\KpzKernels::setGridDx(Int32,Boolean).0.value\) then 
                            \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_4\;
                        else 
                            \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_5\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDx(Int32,Boolean).0._State_3\ => 
                        -- State after the if-else which was started in state \KpzKernels::setGridDx(Int32,Boolean).0._State_2\.
                        -- Since the integer literal 4294967294 was out of the VHDL integer range it was substituted with a binary literal (11111111111111111111111111111110).
                        \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.0\ := \KpzKernels::setGridDx(Int32,Boolean).0.this\.\gridRaw\(to_integer(\KpzKernels::setGridDx(Int32,Boolean).0.index\)) and "11111111111111111111111111111110";
                        \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.1\ := \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.0\ or \KpzKernels::setGridDx(Int32,Boolean).0.conditional8967f26a659a1ddcbabf9fc3ff6f8f89ca99389e2d9f85e3bbeaa263145b233e\;
                        \KpzKernels::setGridDx(Int32,Boolean).0.this\.\gridRaw\(to_integer(\KpzKernels::setGridDx(Int32,Boolean).0.index\)) := \KpzKernels::setGridDx(Int32,Boolean).0.binaryOperationResult.1\;
                        \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                    when \KpzKernels::setGridDx(Int32,Boolean).0._State_4\ => 
                        -- True branch of the if-else started in state \KpzKernels::setGridDx(Int32,Boolean).0._State_2\.
                        \KpzKernels::setGridDx(Int32,Boolean).0.conditional8967f26a659a1ddcbabf9fc3ff6f8f89ca99389e2d9f85e3bbeaa263145b233e\ := to_unsigned(1, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernels::setGridDx(Int32,Boolean).0._State_2\.
                        if (\KpzKernels::setGridDx(Int32,Boolean).0._State\ = \KpzKernels::setGridDx(Int32,Boolean).0._State_4\) then 
                            \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_3\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDx(Int32,Boolean).0._State_5\ => 
                        -- False branch of the if-else started in state \KpzKernels::setGridDx(Int32,Boolean).0._State_2\.
                        \KpzKernels::setGridDx(Int32,Boolean).0.conditional8967f26a659a1ddcbabf9fc3ff6f8f89ca99389e2d9f85e3bbeaa263145b233e\ := to_unsigned(0, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernels::setGridDx(Int32,Boolean).0._State_2\.
                        if (\KpzKernels::setGridDx(Int32,Boolean).0._State\ = \KpzKernels::setGridDx(Int32,Boolean).0._State_5\) then 
                            \KpzKernels::setGridDx(Int32,Boolean).0._State\ := \KpzKernels::setGridDx(Int32,Boolean).0._State_3\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean).0 state machine end


    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean).0 state machine start
    \KpzKernels::setGridDy(Int32,Boolean).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::setGridDy(Int32,Boolean).0._State\: \KpzKernels::setGridDy(Int32,Boolean).0._States\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_0\;
        Variable \KpzKernels::setGridDy(Int32,Boolean).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::setGridDy(Int32,Boolean).0.index\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::setGridDy(Int32,Boolean).0.value\: boolean := false;
        Variable \KpzKernels::setGridDy(Int32,Boolean).0.conditional09e94246096be15388e56e58b4b26eae49dbb4d4f1e9fbfb5ee72677a60c13ae\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.1\: unsigned(31 downto 0) := to_unsigned(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::setGridDy(Int32,Boolean).0._Finished\ <= false;
                \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_0\;
                \KpzKernels::setGridDy(Int32,Boolean).0.index\ := to_signed(0, 32);
                \KpzKernels::setGridDy(Int32,Boolean).0.value\ := false;
                \KpzKernels::setGridDy(Int32,Boolean).0.conditional09e94246096be15388e56e58b4b26eae49dbb4d4f1e9fbfb5ee72677a60c13ae\ := to_unsigned(0, 32);
                \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.0\ := to_unsigned(0, 32);
                \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.1\ := to_unsigned(0, 32);
            else 
                case \KpzKernels::setGridDy(Int32,Boolean).0._State\ is 
                    when \KpzKernels::setGridDy(Int32,Boolean).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::setGridDy(Int32,Boolean).0._Started\ = true) then 
                            \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDy(Int32,Boolean).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::setGridDy(Int32,Boolean).0._Started\ = true) then 
                            \KpzKernels::setGridDy(Int32,Boolean).0._Finished\ <= true;
                        else 
                            \KpzKernels::setGridDy(Int32,Boolean).0._Finished\ <= false;
                            \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::setGridDy(Int32,Boolean).0.this.parameter.Out\ <= \KpzKernels::setGridDy(Int32,Boolean).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDy(Int32,Boolean).0._State_2\ => 
                        \KpzKernels::setGridDy(Int32,Boolean).0.this\ := \KpzKernels::setGridDy(Int32,Boolean).0.this.parameter.In\;
                        \KpzKernels::setGridDy(Int32,Boolean).0.index\ := \KpzKernels::setGridDy(Int32,Boolean).0.index.parameter.In\;
                        \KpzKernels::setGridDy(Int32,Boolean).0.value\ := \KpzKernels::setGridDy(Int32,Boolean).0.value.parameter.In\;

                        -- This if-else was transformed from a .NET if-else. It spans across multiple states:
                        --     * The true branch starts in state \KpzKernels::setGridDy(Int32,Boolean).0._State_4\ and ends in state \KpzKernels::setGridDy(Int32,Boolean).0._State_4\.
                        --     * The false branch starts in state \KpzKernels::setGridDy(Int32,Boolean).0._State_5\ and ends in state \KpzKernels::setGridDy(Int32,Boolean).0._State_5\.
                        --     * Execution after either branch will continue in the following state: \KpzKernels::setGridDy(Int32,Boolean).0._State_3\.

                        if (\KpzKernels::setGridDy(Int32,Boolean).0.value\) then 
                            \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_4\;
                        else 
                            \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_5\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDy(Int32,Boolean).0._State_3\ => 
                        -- State after the if-else which was started in state \KpzKernels::setGridDy(Int32,Boolean).0._State_2\.
                        -- Since the integer literal 4294967293 was out of the VHDL integer range it was substituted with a binary literal (11111111111111111111111111111101).
                        \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.0\ := \KpzKernels::setGridDy(Int32,Boolean).0.this\.\gridRaw\(to_integer(\KpzKernels::setGridDy(Int32,Boolean).0.index\)) and "11111111111111111111111111111101";
                        \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.1\ := \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.0\ or \KpzKernels::setGridDy(Int32,Boolean).0.conditional09e94246096be15388e56e58b4b26eae49dbb4d4f1e9fbfb5ee72677a60c13ae\;
                        \KpzKernels::setGridDy(Int32,Boolean).0.this\.\gridRaw\(to_integer(\KpzKernels::setGridDy(Int32,Boolean).0.index\)) := \KpzKernels::setGridDy(Int32,Boolean).0.binaryOperationResult.1\;
                        \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                    when \KpzKernels::setGridDy(Int32,Boolean).0._State_4\ => 
                        -- True branch of the if-else started in state \KpzKernels::setGridDy(Int32,Boolean).0._State_2\.
                        \KpzKernels::setGridDy(Int32,Boolean).0.conditional09e94246096be15388e56e58b4b26eae49dbb4d4f1e9fbfb5ee72677a60c13ae\ := to_unsigned(2, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernels::setGridDy(Int32,Boolean).0._State_2\.
                        if (\KpzKernels::setGridDy(Int32,Boolean).0._State\ = \KpzKernels::setGridDy(Int32,Boolean).0._State_4\) then 
                            \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_3\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::setGridDy(Int32,Boolean).0._State_5\ => 
                        -- False branch of the if-else started in state \KpzKernels::setGridDy(Int32,Boolean).0._State_2\.
                        \KpzKernels::setGridDy(Int32,Boolean).0.conditional09e94246096be15388e56e58b4b26eae49dbb4d4f1e9fbfb5ee72677a60c13ae\ := to_unsigned(0, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernels::setGridDy(Int32,Boolean).0._State_2\.
                        if (\KpzKernels::setGridDy(Int32,Boolean).0._State\ = \KpzKernels::setGridDy(Int32,Boolean).0._State_5\) then 
                            \KpzKernels::setGridDy(Int32,Boolean).0._State\ := \KpzKernels::setGridDy(Int32,Boolean).0._State_3\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean).0 state machine end


    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine start
    \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\: \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._States\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_0\;
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.i\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.j\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.num\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.0\: boolean := false;
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.1\: boolean := false;
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.2\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.3\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.4\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.5\: signed(31 downto 0) := to_signed(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Finished\ <= false;
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\ <= to_signed(0, 32);
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ <= false;
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_0\;
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.i\ := to_signed(0, 32);
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.j\ := to_signed(0, 32);
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.num\ := to_signed(0, 32);
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.0\ := false;
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.1\ := false;
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.2\ := to_signed(0, 32);
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.3\ := to_signed(0, 32);
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.4\ := to_signed(0, 32);
                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.5\ := to_signed(0, 32);
            else 
                case \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ is 
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Started\ = true) then 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Started\ = true) then 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Finished\ <= true;
                        else 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Finished\ <= false;
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this.parameter.Out\ <= \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_2\ => 
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this.parameter.In\;
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.i\ := to_signed(0, 32);
                        -- Starting a while loop.
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_3\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_3\ => 
                        -- Repeated state of the while loop which was started in state \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_2\.
                        -- The while loop's condition:
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.0\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.i\ < to_signed(8, 32);
                        if (\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.0\) then 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.j\ := to_signed(0, 32);
                            -- Starting a while loop.
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_5\;
                        else 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_4\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_4\ => 
                        -- State after the while loop which was started in state \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_2\.
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_5\ => 
                        -- Repeated state of the while loop which was started in state \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_3\.
                        -- The while loop's condition:
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.1\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.j\ < to_signed(8, 32);
                        if (\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.1\) then 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.2\ := resize(\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.i\ * to_signed(8, 32), 32);
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.3\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.2\ + \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.j\;
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.num\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.3\;
                            -- Begin SimpleMemory write.
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.num\, 32);
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ <= true;
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.DataOut\ <= ConvertUInt32ToStdLogicVector(\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this\.\gridRaw\(to_integer(\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.num\)));
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_7\;
                        else 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_6\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.3
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_6\ => 
                        -- State after the while loop which was started in state \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_3\.
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.5\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.i\ + to_signed(1, 32);
                        \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.i\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.5\;
                        -- Returning to the repeated state of the while loop which was started in state \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_2\ if the loop wasn't exited with a state change.
                        if (\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ = \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_6\) then 
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_3\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_7\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\WritesDone\ = true) then 
                            -- SimpleMemory write finished.
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ <= false;
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.4\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.j\ + to_signed(1, 32);
                            \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.j\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.binaryOperationResult.4\;
                            -- Returning to the repeated state of the while loop which was started in state \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_3\ if the loop wasn't exited with a state change.
                            if (\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ = \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_7\) then 
                                \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._State_5\;
                            end if;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine end


    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine start
    \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\: \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._States\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_0\;
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.i\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.j\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.num\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.0\: boolean := false;
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.1\: boolean := false;
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.2\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.3\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.dataIn.0\: std_logic_vector(31 downto 0);
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.4\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.5\: signed(31 downto 0) := to_signed(0, 32);
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Finished\ <= false;
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\ <= to_signed(0, 32);
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ <= false;
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_0\;
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.i\ := to_signed(0, 32);
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.j\ := to_signed(0, 32);
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.num\ := to_signed(0, 32);
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.0\ := false;
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.1\ := false;
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.2\ := to_signed(0, 32);
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.3\ := to_signed(0, 32);
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.4\ := to_signed(0, 32);
                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.5\ := to_signed(0, 32);
            else 
                case \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ is 
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Started\ = true) then 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Started\ = true) then 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Finished\ <= true;
                        else 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Finished\ <= false;
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this.parameter.Out\ <= \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_2\ => 
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this.parameter.In\;
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.i\ := to_signed(0, 32);
                        -- Starting a while loop.
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_3\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_3\ => 
                        -- Repeated state of the while loop which was started in state \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_2\.
                        -- The while loop's condition:
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.0\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.i\ < to_signed(8, 32);
                        if (\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.0\) then 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.j\ := to_signed(0, 32);
                            -- Starting a while loop.
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_5\;
                        else 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_4\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_4\ => 
                        -- State after the while loop which was started in state \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_2\.
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_5\ => 
                        -- Repeated state of the while loop which was started in state \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_3\.
                        -- The while loop's condition:
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.1\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.j\ < to_signed(8, 32);
                        if (\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.1\) then 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.2\ := resize(\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.j\ * to_signed(8, 32), 32);
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.3\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.2\ + \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.i\;
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.num\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.3\;
                            -- Begin SimpleMemory read.
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\ <= resize(\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.num\, 32);
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ <= true;
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_7\;
                        else 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_6\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.3
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_6\ => 
                        -- State after the while loop which was started in state \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_3\.
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.5\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.i\ + to_signed(1, 32);
                        \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.i\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.5\;
                        -- Returning to the repeated state of the while loop which was started in state \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_2\ if the loop wasn't exited with a state change.
                        if (\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ = \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_6\) then 
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_3\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_7\ => 
                        -- Waiting for the SimpleMemory operation to finish.
                        if (\ReadsDone\ = true) then 
                            -- SimpleMemory read finished.
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ <= false;
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.dataIn.0\ := \DataIn\;
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this\.\gridRaw\(to_integer(\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.num\)) := ConvertStdLogicVectorToUInt32(\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.dataIn.0\);
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.4\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.j\ + to_signed(1, 32);
                            \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.j\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.binaryOperationResult.4\;
                            -- Returning to the repeated state of the while loop which was started in state \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_3\ if the loop wasn't exited with a state change.
                            if (\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ = \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_7\) then 
                                \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State\ := \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._State_5\;
                            end if;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 state machine end


    -- System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 state machine start
    \KpzKernels::RandomlySwitchFourCells(Boolean).0._StateMachine\: process (\Clock\) 
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\: \KpzKernels::RandomlySwitchFourCells(Boolean).0._States\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_0\;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\: \Hast.Samples.Kpz.KpzKernels\;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num2\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom2\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num3\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num4\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num5\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num6\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num7\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.num8\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.index\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.index2\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.flag\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.0\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.0\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.1\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.2\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.1\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.2\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.3\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.4\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.5\: unsigned(31 downto 0) := to_unsigned(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.6\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.7\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.8\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.9\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.10\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.11\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.12\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.13\: signed(31 downto 0) := to_signed(0, 32);
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.3\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.4\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.14\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.5\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.15\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.6\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.16\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.17\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.18\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.19\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.7\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.8\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.20\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.9\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.21\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.10\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.22\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.23\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.24\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.25\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.26\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.11\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.12\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.13\: boolean := false;
        Variable \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.14\: boolean := false;
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \KpzKernels::RandomlySwitchFourCells(Boolean).0._Finished\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Started.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).x.parameter.Out.0\ <= to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).y.parameter.Out.0\ <= to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Started.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Started.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\ <= to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\ <= to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).index.parameter.Out.0\ <= to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).value.parameter.Out.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).index.parameter.Out.0\ <= to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).value.parameter.Out.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\ <= false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_0\;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num2\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom2\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num3\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num4\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num5\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num6\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num7\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.num8\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.index\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.index2\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.flag\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.0\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.0\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.1\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.2\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.1\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.2\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.3\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.4\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.5\ := to_unsigned(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.6\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.7\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.8\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.9\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.10\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.11\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.12\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.13\ := to_signed(0, 32);
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.3\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.4\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.14\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.5\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.15\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.6\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.16\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.17\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.18\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.19\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.7\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.8\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.20\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.9\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.21\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.10\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.22\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.23\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.24\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.25\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.26\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.11\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.12\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.13\ := false;
                \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.14\ := false;
            else 
                case \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ is 
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_0\ => 
                        -- Start state
                        -- Waiting for the start signal.
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0._Started\ = true) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_2\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_1\ => 
                        -- Final state
                        -- Signaling finished until Started is pulled back to false, then returning to the start state.
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0._Started\ = true) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._Finished\ <= true;
                        else 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._Finished\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_0\;
                        end if;
                        -- Writing back out-flowing parameters so any changes made in this state machine will be reflected in the invoking one too.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.this.parameter.Out\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_2\ => 
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.this.parameter.In\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch.parameter.In\;
                        -- Starting state machine invocation for the following method: System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1()
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Started.0\ <= true;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_3\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_3\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1()
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.0\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.0\ := signed(\KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom\ and to_unsigned(7, 32));
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.num\ := (\KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.0\);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.1\ := shift_right(\KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom\, to_integer(to_signed(16, 32)));
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.2\ := signed(\KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.1\ and to_unsigned(7, 32));
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.num2\ := (\KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.2\);
                            -- Starting state machine invocation for the following method: System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).x.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.num\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).y.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.num2\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_4\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.3
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_4\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.1\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.1\;
                            -- Starting state machine invocation for the following method: System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2()
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2()
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.2\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom2\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.2\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.3\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom2\ and to_unsigned(65535, 32);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.num3\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.3\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.4\ := shift_right(\KpzKernels::RandomlySwitchFourCells(Boolean).0.nextRandom2\, to_integer(to_signed(16, 32)));
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.5\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.4\ and to_unsigned(65535, 32);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.num4\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.5\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.6\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num\ < to_signed(7, 32);

                            -- This if-else was transformed from a .NET if-else. It spans across multiple states:
                            --     * The true branch starts in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_7\ and ends in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_7\.
                            --     * The false branch starts in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_8\ and ends in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_8\.
                            --     * Execution after either branch will continue in the following state: \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\.

                            if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.6\) then 
                                \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_7\;
                            else 
                                \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_8\;
                            end if;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.4
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\ => 
                        -- State after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.num6\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num2\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.num7\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.8\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num2\ < to_signed(7, 32);

                        -- This if-else was transformed from a .NET if-else. It spans across multiple states:
                        --     * The true branch starts in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_10\ and ends in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_10\.
                        --     * The false branch starts in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_11\ and ends in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_11\.
                        --     * Execution after either branch will continue in the following state: \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_9\.

                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.8\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_10\;
                        else 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_11\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_7\ => 
                        -- True branch of the if-else started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.7\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num\ + to_signed(1, 32);
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.num5\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.7\;
                        -- Going to the state after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\.
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_7\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_8\ => 
                        -- False branch of the if-else started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.num5\ := to_signed(0, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_5\.
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_8\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_9\ => 
                        -- State after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.10\ := resize(\KpzKernels::RandomlySwitchFourCells(Boolean).0.num6\ * to_signed(8, 32), 32);
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.11\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.10\ + \KpzKernels::RandomlySwitchFourCells(Boolean).0.num5\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.index\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.11\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.12\ := resize(\KpzKernels::RandomlySwitchFourCells(Boolean).0.num8\ * to_signed(8, 32), 32);
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.13\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.12\ + \KpzKernels::RandomlySwitchFourCells(Boolean).0.num7\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.index2\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.13\;
                        -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= true;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_12\;
                        -- Clock cycles needed to complete this state (approximation): 0.4
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_10\ => 
                        -- True branch of the if-else started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.9\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num2\ + to_signed(1, 32);
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.num8\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.9\;
                        -- Going to the state after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\.
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_10\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_9\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_11\ => 
                        -- False branch of the if-else started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.num8\ := to_signed(0, 32);
                        -- Going to the state after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_6\.
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_11\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_9\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_12\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.3\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\;
                            -- The last invocation for the target state machine just finished, so need to start the next one in a later state.
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_13\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_13\ => 
                        -- This state was just added to leave time for the invocation proxy to register that the previous invocation finished.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_14\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_14\ => 
                        -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= true;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_15\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_15\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.4\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.14\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.3\ and not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.4\);
                            -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_16\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_16\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.5\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.15\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.14\ and \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.5\;
                            -- The last invocation for the target state machine just finished, so need to start the next one in a later state.
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_17\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_17\ => 
                        -- This state was just added to leave time for the invocation proxy to register that the previous invocation finished.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_18\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_18\ => 
                        -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index2\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= true;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_19\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_19\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.6\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.16\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.15\ and not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.6\);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.17\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num3\ < \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\.\integerProbabilityP\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.18\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch\ or \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.17\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.19\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.16\ and \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.18\;
                            -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_20\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.5
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_20\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.7\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\;
                            -- The last invocation for the target state machine just finished, so need to start the next one in a later state.
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_21\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_21\ => 
                        -- This state was just added to leave time for the invocation proxy to register that the previous invocation finished.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_22\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_22\ => 
                        -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= true;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_23\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_23\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.8\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.20\ := not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.7\) and \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.8\;
                            -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_24\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.2
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_24\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.9\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.21\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.20\ and not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.9\);
                            -- The last invocation for the target state machine just finished, so need to start the next one in a later state.
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_25\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_25\ => 
                        -- This state was just added to leave time for the invocation proxy to register that the previous invocation finished.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_26\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_26\ => 
                        -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index2\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= true;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_27\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_27\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.10\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.22\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.21\ and \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.10\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.23\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.num4\ < \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\.\integerProbabilityQ\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.24\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch\ or \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.23\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.25\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.22\ and \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.24\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.26\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.19\ or \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.25\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.flag\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.binaryOperationResult.26\;

                            -- This if-else was transformed from a .NET if-else. It spans across multiple states:
                            --     * The true branch starts in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_29\ and ends in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_37\.
                            --     * Execution after either branch will continue in the following state: \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_28\.

                            if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.flag\) then 
                                \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_29\;
                            else 
                                -- There was no false branch, so going directly to the state after the if-else.
                                \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_28\;
                            end if;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.5
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_28\ => 
                        -- State after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_27\.
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_1\;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_29\ => 
                        -- True branch of the if-else started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_27\.
                        -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= true;
                        \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_30\;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_30\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.11\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).value.parameter.Out.0\ <= not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.11\);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_31\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_31\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_32\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_32\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.12\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.indexFromXY\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).value.parameter.Out.0\ <= not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.12\);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_33\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_33\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_34\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_34\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.13\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).value.parameter.Out.0\ <= not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.13\);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_35\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_35\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index2\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_36\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0.1
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_36\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.return.14\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\;
                            -- Starting state machine invocation for the following method: System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean)
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).index.parameter.Out.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.index2\;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).value.parameter.Out.0\ <= not(\KpzKernels::RandomlySwitchFourCells(Boolean).0.return.14\);
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\ <= true;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_37\;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                    when \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_37\ => 
                        -- Waiting for the state machine invocation of the following method to finish: System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean)
                        if (\KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Finished.0\) then 
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\ <= false;
                            \KpzKernels::RandomlySwitchFourCells(Boolean).0.this\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.In.0\;
                            -- Going to the state after the if-else which was started in state \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_27\.
                            if (\KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ = \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_37\) then 
                                \KpzKernels::RandomlySwitchFourCells(Boolean).0._State\ := \KpzKernels::RandomlySwitchFourCells(Boolean).0._State_28\;
                            end if;
                        end if;
                        -- Clock cycles needed to complete this state (approximation): 0
                end case;
            end if;
        end if;
    end process;
    -- System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 state machine end


    -- System.Void Hast::ExternalInvocationProxy() start
    \Finished\ <= \FinishedInternal\;
    \Hast::ExternalInvocationProxy()\: process (\Clock\) 
    begin 
        if (rising_edge(\Clock\)) then 
            if (\Reset\ = '1') then 
                -- Synchronous reset
                \FinishedInternal\ <= false;
                \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Started.0\ <= false;
                \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Started.0\ <= false;
            else 
                if (\Started\ = true and \FinishedInternal\ = false) then 
                    -- Starting the state machine corresponding to the given member ID.
                    case \MemberId\ is 
                        when 0 => 
                            if (\Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Started.0\ = false) then 
                                \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Started.0\ <= true;
                            elsif (\Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Started.0\ = \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Finished.0\) then 
                                \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Started.0\ <= false;
                                \FinishedInternal\ <= true;
                            end if;
                        when 1 => 
                            if (\Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Started.0\ = false) then 
                                \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Started.0\ <= true;
                            elsif (\Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Started.0\ = \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Finished.0\) then 
                                \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Started.0\ <= false;
                                \FinishedInternal\ <= true;
                            end if;
                        when others => 
                            null;
                    end case;
                else 
                    -- Waiting for Started to be pulled back to zero that signals the framework noting the finish.
                    if (\Started\ = false and \FinishedInternal\ = true) then 
                        \FinishedInternal\ <= false;
                    end if;
                end if;
            end if;
        end if;
    end process;
    -- System.Void Hast::ExternalInvocationProxy() end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 (#0):
    \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Started\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Started.0\;
    \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this.parameter.In\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).this.parameter.Out.0\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory)._Finished.0\ <= \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0._Finished\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).this.parameter.In.0\ <= \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::CopyFromSimpleMemoryToRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 (#0):
    \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Started\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Started.0\;
    \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this.parameter.In\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory).this.parameter.Out.0\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory)._Finished.0\ <= \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0._Finished\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::InitializeParametersFromMemory(SimpleMemory).this.parameter.In.0\ <= \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::InitializeParametersFromMemory(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 (#0):
    \KpzKernels::RandomlySwitchFourCells(Boolean).0._Started\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Started.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.this.parameter.In\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).this.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.forceSwitch.parameter.In\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).forceSwitch.parameter.Out.0\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean)._Finished.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0._Finished\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::RandomlySwitchFourCells(Boolean).this.parameter.In.0\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean) end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory).0 (#0):
    \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Started\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Started.0\;
    \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this.parameter.In\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).this.parameter.Out.0\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory)._Finished.0\ <= \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0._Finished\;
    \KpzKernelsInterface::DoIterations(SimpleMemory).0.KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).this.parameter.In.0\ <= \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::CopyToSimpleMemoryFromRawGrid(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) end


    -- System.Void Hast::InternalInvocationProxy().System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1() start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 (#0):
    \KpzKernels::GetNextRandom1().0._Started\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Started.0\;
    \KpzKernels::GetNextRandom1().0.this.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().this.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1()._Finished.0\ <= \KpzKernels::GetNextRandom1().0._Finished\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().return.0\ <= \KpzKernels::GetNextRandom1().0.return\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom1().this.parameter.In.0\ <= \KpzKernels::GetNextRandom1().0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom1() end


    -- System.Void Hast::InternalInvocationProxy().System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 (#0):
    \KpzKernels::getIndexFromXY(Int32,Int32).0._Started\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Started.0\;
    \KpzKernels::getIndexFromXY(Int32,Int32).0.this.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).this.parameter.Out.0\;
    \KpzKernels::getIndexFromXY(Int32,Int32).0.x.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).x.parameter.Out.0\;
    \KpzKernels::getIndexFromXY(Int32,Int32).0.y.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).y.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32)._Finished.0\ <= \KpzKernels::getIndexFromXY(Int32,Int32).0._Finished\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).return.0\ <= \KpzKernels::getIndexFromXY(Int32,Int32).0.return\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getIndexFromXY(Int32,Int32).this.parameter.In.0\ <= \KpzKernels::getIndexFromXY(Int32,Int32).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Int32 Hast.Samples.Kpz.KpzKernels::getIndexFromXY(System.Int32,System.Int32) end


    -- System.Void Hast::InternalInvocationProxy().System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2() start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 (#0):
    \KpzKernels::GetNextRandom2().0._Started\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Started.0\;
    \KpzKernels::GetNextRandom2().0.this.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().this.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2()._Finished.0\ <= \KpzKernels::GetNextRandom2().0._Finished\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().return.0\ <= \KpzKernels::GetNextRandom2().0.return\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::GetNextRandom2().this.parameter.In.0\ <= \KpzKernels::GetNextRandom2().0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.UInt32 Hast.Samples.Kpz.KpzKernels::GetNextRandom2() end


    -- System.Void Hast::InternalInvocationProxy().System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 (#0):
    \KpzKernels::getGridDx(Int32).0._Started\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Started.0\;
    \KpzKernels::getGridDx(Int32).0.this.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.Out.0\;
    \KpzKernels::getGridDx(Int32).0.index.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).index.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32)._Finished.0\ <= \KpzKernels::getGridDx(Int32).0._Finished\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).return.0\ <= \KpzKernels::getGridDx(Int32).0.return\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDx(Int32).this.parameter.In.0\ <= \KpzKernels::getGridDx(Int32).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDx(System.Int32) end


    -- System.Void Hast::InternalInvocationProxy().System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 (#0):
    \KpzKernels::getGridDy(Int32).0._Started\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Started.0\;
    \KpzKernels::getGridDy(Int32).0.this.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.Out.0\;
    \KpzKernels::getGridDy(Int32).0.index.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).index.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32)._Finished.0\ <= \KpzKernels::getGridDy(Int32).0._Finished\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).return.0\ <= \KpzKernels::getGridDy(Int32).0.return\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::getGridDy(Int32).this.parameter.In.0\ <= \KpzKernels::getGridDy(Int32).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Boolean Hast.Samples.Kpz.KpzKernels::getGridDy(System.Int32) end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 (#0):
    \KpzKernels::setGridDx(Int32,Boolean).0._Started\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Started.0\;
    \KpzKernels::setGridDx(Int32,Boolean).0.this.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.Out.0\;
    \KpzKernels::setGridDx(Int32,Boolean).0.index.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).index.parameter.Out.0\;
    \KpzKernels::setGridDx(Int32,Boolean).0.value.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).value.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean)._Finished.0\ <= \KpzKernels::setGridDx(Int32,Boolean).0._Finished\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDx(Int32,Boolean).this.parameter.In.0\ <= \KpzKernels::setGridDx(Int32,Boolean).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::setGridDx(System.Int32,System.Boolean) end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean) start
    -- Signal connections for System.Void Hast.Samples.Kpz.KpzKernels::RandomlySwitchFourCells(System.Boolean).0 (#0):
    \KpzKernels::setGridDy(Int32,Boolean).0._Started\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Started.0\;
    \KpzKernels::setGridDy(Int32,Boolean).0.this.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.Out.0\;
    \KpzKernels::setGridDy(Int32,Boolean).0.index.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).index.parameter.Out.0\;
    \KpzKernels::setGridDy(Int32,Boolean).0.value.parameter.In\ <= \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).value.parameter.Out.0\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean)._Finished.0\ <= \KpzKernels::setGridDy(Int32,Boolean).0._Finished\;
    \KpzKernels::RandomlySwitchFourCells(Boolean).0.KpzKernels::setGridDy(Int32,Boolean).this.parameter.In.0\ <= \KpzKernels::setGridDy(Int32,Boolean).0.this.parameter.Out\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernels::setGridDy(System.Int32,System.Boolean) end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) start
    -- Signal connections for System.Void Hast::ExternalInvocationProxy() (#0):
    \KpzKernelsInterface::DoIterations(SimpleMemory).0._Started\ <= \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Started.0\;
    \Hast::ExternalInvocationProxy().KpzKernelsInterface::DoIterations(SimpleMemory)._Finished.0\ <= \KpzKernelsInterface::DoIterations(SimpleMemory).0._Finished\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernelsInterface::DoIterations(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) end


    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernelsInterface::TestAdd(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) start
    -- Signal connections for System.Void Hast::ExternalInvocationProxy() (#0):
    \KpzKernelsInterface::TestAdd(SimpleMemory).0._Started\ <= \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Started.0\;
    \Hast::ExternalInvocationProxy().KpzKernelsInterface::TestAdd(SimpleMemory)._Finished.0\ <= \KpzKernelsInterface::TestAdd(SimpleMemory).0._Finished\;
    -- System.Void Hast::InternalInvocationProxy().System.Void Hast.Samples.Kpz.KpzKernelsInterface::TestAdd(Hast.Transformer.Abstractions.SimpleMemory.SimpleMemory) end


    -- System.Void Hast::SimpleMemoryOperationProxy() start
    \CellIndex\ <= to_integer(\KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.CellIndex\) when \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\ or \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.WriteEnable\ else to_integer(\KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.CellIndex\) when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ or \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.WriteEnable\ else to_integer(\KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\) when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ or \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ else to_integer(\KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.CellIndex\) when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ or \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ else 0;
    \DataOut\ <= \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.DataOut\ when \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.WriteEnable\ else \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.DataOut\ when \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.WriteEnable\ else \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.DataOut\ when \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ else \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.DataOut\ when \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ else "00000000000000000000000000000000";
    \ReadEnable\ <= \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.ReadEnable\ or \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ or \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.ReadEnable\ or \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.ReadEnable\;
    \WriteEnable\ <= \KpzKernels::InitializeParametersFromMemory(SimpleMemory).0.SimpleMemory.WriteEnable\ or \KpzKernels::CopyToSimpleMemoryFromRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ or \KpzKernels::CopyFromSimpleMemoryToRawGrid(SimpleMemory).0.SimpleMemory.WriteEnable\ or \KpzKernelsInterface::TestAdd(SimpleMemory).0.SimpleMemory.WriteEnable\;
    -- System.Void Hast::SimpleMemoryOperationProxy() end

end Imp;
