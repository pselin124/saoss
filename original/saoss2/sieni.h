#ifndef sieni_h_i
#define sieni_h_i

#define SSADE 5

typedef struct{
	int x,y;
	bool uusiks;
}SPiste;

extern float skuolemapisteet;
extern SPiste *spiste;
extern int spisteita;
void alustaSPisteet();
void laskeSPisteet();
void piirraSPisteet();
void piirraSGraffat();

#endif