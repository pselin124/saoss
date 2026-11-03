#ifndef ctf_h_i
#define ctf_h_i

#define CSADE 7

typedef struct{
	float x,y;
	char player,numero;
	bool enabled;

}CPiste;

extern float ckuolemapisteet;
extern CPiste *cpiste, *alkupiste;
extern int cpisteita;
void alustaCPisteet();
void laskeCPisteet();
void piirraCPisteet();
void piirraCGraffat();

#endif