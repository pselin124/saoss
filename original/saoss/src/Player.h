#ifndef _player_h_included_
#define _player_h_included_
#include "Alus.h"
#include "Pati.h"
#include "Kaasujalki.h"
#include "Viivakasa.h"
#include "sirpale.h"
#include <SDL.h>
//#include <SDL_mixer.h>
#include "audio.h"
//#include <main.h>
class Player{
public:
	Player();
	~Player();
	Sound *ampumisaani;
	Sound *osumisaani;
	Sound *eampumisaani;
	Sound *eosumisaani;
	void ailiiku();
	float uusx, uusy;
	bool teleportattu;
	void tarkistaHajosikoPadit();
	float propelli;
	void piirra(int x, int y, int z, int kulma, int asekulma);
	void piirra();
	void liiku();
	void ammu();
	void liikutaPadit();
	void asetaAlus(Alus alus2);
	void uudestaan(Alus alus2);
	void piirraPadit();
	void piirraePadit();
	void liikutaePadit();
	int ampumispuoli;
	Viivakasa *viivat;
	float pisteet;
	void piirraViivat();
	void laskeViivat();
	void laitaAlkuSirpaleet();
	void laskeAlkuSirpaleet();
	void piirraAlkuSirpaleet();
	bool tormaa(double x2, double y2, double sade2, int i);
	bool tormaa(Pati pati2, Alus alus2);
	float	x, y, kulma, xvauhti,yvauhti, alpha,asekulma;
	float energia, fuel;
	int kaasujalkiaika;
	bool pylos, palas, pleft, pright, pfire, pease;
	Alus alus;
	int numero;
	int ampumisviive;
	int limacount;
	Pati *pati, *epati, *lima;
	Sirpale *alkusirpale;
	Kaasujalki *kaasujalki, *kaasujalki2;
	void lisaalima();
	void laskeKaasujaljet();
	void piirraKaasujaljet();
	void piirraGraffat();
	void piirraLimat();
	void ease();
	bool suojapaalla;
	float easekesto;
	float easeviive;
	bool laseraani;
	short kuolema;
	float ampumisjuttu;
	float fcounter;
	float xhanta[palikkamaara],yhanta[palikkamaara],kulmahanta[palikkamaara];
	bool ihmispelaaja;
	int tiimi;
	bool kaasu, joinease, joinase,joinleft,joinright;

private:
//	GLfloat v0[3], v1[3],v2[3], v3[3], v4[3], v5[3], v6[3], v7[3];

};
#endif

