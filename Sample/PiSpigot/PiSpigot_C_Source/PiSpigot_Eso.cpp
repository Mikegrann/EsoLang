#include <iostream>
#include "PiSpigot.hpp"
#include "Util.hpp"

/* Implementation is meant to reflect the execution of the EsoLang program. 
   This is why many variables are global, operations are function calls from
   Util, and recursion is used instead of looping. */

int len, numNines, predigit, q;
int *Arr;

// Print to screen for when q == 10
void outputTen() {
	// <C-specific>
		std::cout << predigit + 1;

		for (int k = 0; k < numNines; ++k) { std::cout << 0; }
	// </C-specific>

	predigit = zero;
	numNines = zero;
}

// Print to screen for when q == 9
void outputElse() {
	// <C-specific>
		std::cout << predigit;
	// </C-specific>

	predigit = q;

	if (numNines > 0) {
		// <C-specific>
			for (int k = 0; k < numNines; ++k) { std::cout << 9; }
		// </C-specific>

		numNines = zero;
	}
	else { }
}

// "Loop" through from i -> 0 doing matrix operations
void innerLoop(int i) {
	if (i > 0) {
		int idx = i + (-one);
		int entry = Arr[idx];
		int x = Mult(10, entry) + Mult(q, i);
		int divisor = Mult(2, i) + (-one);

		Arr[idx] = Mod(x, divisor);
		q = Div(x, divisor);
		
		i = i + (-one);
		innerLoop(i);
	}
	else { }
}

// "Loop" through from j -> loopbound finding the next digit and outputting
void outerLoop(int j) {
	int loopbound = numDigits + 1;

	if (Lt(j, loopbound) > 0) {
		q = zero;

		innerLoop(len);

		Arr[zero] = Mod(q, 10);
		q = Div(q, 10);

		if (Eq(q, 9) > 0) {
			numNines = numNines + 1;
		}
		else {
			if (Eq(q, 10) > 0) {
				outputTen();
			}
			else {
				outputElse();
			}
		}

		j = j + 1;
		outerLoop(j);
	}
	else { }
}

// Setup some variables and execute the main loop
void PiSpigot_Eso() {
	len = Mult(10, numDigits);
	len = Div(len, 3);
	len = len + one;

	Arr = new int[len];
	
	// <C-specific>
		for (int i = 0; i < len; i++) { Arr[i] = 2; }
	// </C-specific>

	outerLoop(one);

	// <C-specific>
		std::cout << predigit;
		delete Arr;
	// </C-specific>
}