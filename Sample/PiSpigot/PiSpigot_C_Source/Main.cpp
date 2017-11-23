#include <iostream>
#include "Test.hpp"
#include "PiSpigot.hpp"

int main() {

	// Run Tests
	TestAll();

	// Print each implementation on separate lines
	PiSpigot_Eso();
	std::cout << std::endl;
	PiSpigot_C();

	return 0;
}