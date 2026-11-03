#ifndef dominition_h_i
#define dominition_h_i

#define DSADE 10

typedef struct{
	int x,y,tiimi;

}DPiste;

extern DPiste *dpiste;
extern int dpisteita;
void alustaDPisteet();
void laskeDPisteet();
void piirraDPisteet();
void piirraDGraffat();

#endif