#ifndef _SCENE_H_INCLUDED_
#define _SCENE_H_INCLUDED_

void calculateFrame(int framecount);
void drawFrame(int framecount);
void valitseAlus(int numero);
void drawAlusValinta(int numero, int alusnumero);
void drawPlanet();
void laskePikkumiehet();
void piirraPikkumiehet();
void piirraAsteroidit();
void laskeAsteroidit();
extern float alfa,beta,alfa2,beta2;
#endif