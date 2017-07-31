library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

package TypeConversion is
	function Truncate(input: unsigned; size: natural) return unsigned;
	function Truncate(input: signed; size: natural) return signed;
	function ToUnsignedAndExpand(input: signed; size: natural) return unsigned;
end TypeConversion;
		
package body TypeConversion is

	function Truncate(input: unsigned; size: natural) return unsigned is
	begin
		return input(size - 1 downto 0);
	end Truncate;

	function Truncate(input: signed; size: natural) return signed is
	begin
		return input(size - 1 downto 0);
	end Truncate;

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
