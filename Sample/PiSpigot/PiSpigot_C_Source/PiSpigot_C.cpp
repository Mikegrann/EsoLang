#include <iostream>
#include "PiSpigot.hpp"

// A purely C implementation of PiSpigot
// Credit: Pedro Silva
// http://stackoverflow.com/questions/4084571/implementing-the-spigot-algorithm-for-%CF%80-pi
// Used to generate expected output against which PiSpigot_Eso is tested

void PiSpigot_C() {
	int len = (10 * numDigits / 3) + 1;
	int *A = new int[len];

	for(int i = 0; i < len; ++i) {
		A[i] = 2;
	}

	int nines    = 0;
	int predigit = 0;

	for(int j = 1; j < numDigits + 1; ++j) {        
		int q = 0;

		for(int i = len; i > 0; --i) {
			int x  = 10 * A[i-1] + q*i;
			A[i-1] = x % (2*i - 1);
			q = x / (2*i - 1);
		}

		A[0] = q%10;
		q    = q/10;

		if (9 == q) {
			++nines;
		}
		else if (10 == q) {
			std::cout << predigit + 1;

			for (int k = 0; k < nines; ++k) {
				std::cout << 0;
			}
			predigit, nines = 0;
		}
		else {
			std::cout << predigit;
			predigit = q;

			if (0 != nines) {    
				for (int k = 0; k < nines; ++k) {
					std::cout << 9;
				}

				nines = 0;
			}
		}
	}
	std::cout << predigit;

	delete A;
}