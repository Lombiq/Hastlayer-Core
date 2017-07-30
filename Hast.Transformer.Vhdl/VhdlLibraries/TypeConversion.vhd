library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

package TypeConversion is
	function Truncate(input: unsigned; size: natural) return unsigned;
	function Truncate(input: signed; size: natural) return signed;
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

end TypeConversion;
