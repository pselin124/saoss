#include "main.h"
#include <GL/gl.h>
#include <GL/glu.h>


void valitseVari(int tiimi){
		switch(tiimi){
		case 0:
			glColor4f(1.0f,0.25f,0.25f,2.0f);		
			break;
		case 1:
			glColor4f(0.25f,0.25f,1.0f,2.0f);		
			break;
		case 2:
			glColor4f(0.25f,1.0f,0.25f,2.0f);		
			break;
		case 3:
			glColor4f(0.0f,1.0f,1.0f,1.0f);		
			break;
		case 4:
			glColor4f(1.0f,1.0f,1.0f,1.0f);		
			break;
		case 5:
			glColor4f(1.0f,0.0f,1.0f,1.0f);		
			break;
		case 6:
			glColor4f(1.0f,1.0f,0.0f,1.0f);		
			break;
		case 7:
			glColor4f(1.0f,0.6f,0.6f,1.0f);		
			break;
		case 8:
			glColor4f(0.6f,1.0f,0.6f,1.0f);		
			break;
		case 9:
			glColor4f(0.6f,0.6f,1.0f,1.0f);		
			break;
		case 10:
			glColor4f(1.0f,0.6f,1.0f,1.0f);		
			break;
		case 11:
			glColor4f(1.0f,1.0f,0.6f,1.0f);		
			break;
		case 12:
			glColor4f(0.6f,1.0f,1.0f,1.0f);		
			break;
		case 13:
			glColor4f(1.0f,0.0f,0.0f,1.0f);		
			break;
		case 14:
			glColor4f(0.0f,1.0f,0.0f,1.0f);		
			break;
		case 15:
			glColor4f(0.0f,0.0f,1.0f,1.0f);		
			break;
		default:
			glColor4f(1.0f,0.0f,0.0f,1.0f);		
			break;
		}

}
void drawCube(){
	glBegin(GL_QUADS);
		//Front Face
		glNormal3f(0, 0, 1);
		glVertex3f(-1, -1, 1);
		glVertex3f( 1, -1, 1);	
		glVertex3f( 1,  1, 1);	
		glVertex3f(-1,  1, 1);
		// Back Face
		glNormal3f(0, 0, -1);
		glVertex3f(-1, -1, -1);	
		glVertex3f(-1,  1, -1);	
		glVertex3f( 1,  1, -1);	
		glVertex3f( 1, -1, -1);	
		// Top Face
		glNormal3f(0, 1, 0);
		glVertex3f(-1,  1, -1);	
		glVertex3f(-1,  1,  1);	
		glVertex3f( 1,  1,  1);	
		glVertex3f( 1,  1, -1);	
		// Bottom Face
		glNormal3f(0, -1, 0);
		glVertex3f(-1, -1, -1);
		glVertex3f( 1, -1, -1);
		glVertex3f( 1, -1,  1);	
		glVertex3f(-1, -1,  1);
		// Right face
		glNormal3f(1, 0, 0);
		glVertex3f(1, -1, -1);
		glVertex3f(1,  1, -1);
		glVertex3f(1,  1,  1);	
		glVertex3f(1, -1,  1);
		// Left Face
		glNormal3f(-1, 0, 0);
		glVertex3f(-1, -1, -1);
		glVertex3f(-1, -1,  1);
		glVertex3f(-1,  1,  1);	
		glVertex3f(-1,  1, -1);
	glEnd();
}

void drawTri(){
	glBegin(GL_TRIANGLES);
		//top
		glNormal3f(0, 0, 1);
		glVertex3f(-1, -1, 1);//a
		glVertex3f( 1, -1, 1);//b
		glVertex3f( 0,  1, 1);//c
		//bottom
		glNormal3f(0, 0, -1);
		glVertex3f(-1, -1, -1);//d
		glVertex3f( 1, -1, -1);//e
		glVertex3f( 0,  1, -1);//f
	glEnd();
	glBegin(GL_QUADS);
		//leftup
		glNormal3f(-1, 1, 0);
		glVertex3f(-1, -1, -1);//d
		glVertex3f(-1, -1,  1);//a
		glVertex3f( 0,  1,  1);//c
		glVertex3f( 0,  1, -1);//f	
		//front
		glNormal3f(0, -1, 0);
		glVertex3f(-1, -1, -1);//d
		glVertex3f( 1, -1, -1);//e
		glVertex3f( 1, -1,  1);//b
		glVertex3f(-1, -1,  1);//a
		//rightup
		glNormal3f(1, 1, 0);
		glVertex3f(1, -1, -1);
		glVertex3f(0,  1, -1);
		glVertex3f(0,  1,  1);	
		glVertex3f(1, -1,  1);
		// Left Face
/*		glNormal3f(-1, 0, 0);
		glVertex3f(-1, -1, -1);
		glVertex3f(-1, -1,  1);
		glVertex3f(-1,  1,  1);	
		glVertex3f(-1,  1, -1);*/
	glEnd();
}

void drawTri2(){
	glBegin(GL_TRIANGLES);
		//Front Face
		glNormal3f(0, 0, 1);
		glVertex3f(-1, -1, 1);
		glVertex3f( 0, -1, 1);	
		glVertex3f( 0,  1, 1);	
		// Back Face
		glNormal3f(0, 0, -1);
		glVertex3f(-1, -1, -1);	
		glVertex3f( 0,  1, -1);	
		glVertex3f( 0, -1, -1);	
	glEnd();
	glBegin(GL_QUADS);
		// Top Face
		glNormal3f(-1, 1, 0);
		glVertex3f(-1, -1, -1);	
		glVertex3f(-1, -1,  1);	
		glVertex3f( 0,  1,  1);	
		glVertex3f( 0,  1, -1);	
		// Bottom Face
		glNormal3f(0, -1, 0);
		glVertex3f(-1, -1, -1);
		glVertex3f( 0, -1, -1);
		glVertex3f( 0, -1,  1);	
		glVertex3f(-1, -1,  1);
		// Right face
		glNormal3f(1, 0, 0);
		glVertex3f(0, -1, -1);
		glVertex3f(0,  1, -1);
		glVertex3f(0,  1,  1);	
		glVertex3f(0, -1,  1);
		// Left Face
/*		glNormal3f(-1, 0, 0);
		glVertex3f(-1, -1, -1);
		glVertex3f(-1, -1,  1);
		glVertex3f(-1,  1,  1);	
		glVertex3f(-1,  1, -1);*/
	glEnd();
}

void generateLightFlare(GLuint *texture){
	glBindTexture(GL_TEXTURE_2D,*texture);
	float texturedata[64*64];
	int x,y,px,py;
	float falloff=0.005;
	for (y=0;y<64;y++){
		for (x=0;x<64;x++){
			px=x-32;
			py=y-32;
			float r=px*px+py*py;

			float col=exp(-r*falloff)-0.01;

			texturedata[y*64+x]=col;
		}
	}
	glTexImage2D(GL_TEXTURE_2D,0,1,64,64,0,GL_LUMINANCE,GL_FLOAT,texturedata);
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);
}

void generateLightNelio(GLuint *texture){
	glBindTexture(GL_TEXTURE_2D,*texture);
	float texturedata[16*16];
	int i;
	for(i=0;i<16*16;i++){
		texturedata[i]=1;
	}
	glTexImage2D(GL_TEXTURE_2D,0,1,16,16,0,GL_LUMINANCE,GL_FLOAT,texturedata);
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);
}

GLuint tiimitexture;

void generateTeamJuttu(GLuint *texture){
	glBindTexture(GL_TEXTURE_2D,*texture);
	float texturedata[64*64];
	int x,y,px,py;
	float falloff=0.005;
	for (y=0;y<64;y++){
		for (x=0;x<64;x++){
			px=x-32;
			py=y-32;
/*			if(sqrt(px*px+py*py)<16){
				px=16-abs(px);
				py=16-abs(py);
				if(px<0)px=0;
				if(py<0)py=0;
			}*/
			float r=px*px+py*py;

			float col=exp(-r*falloff)-0.01;
			texturedata[y*64+x]=col*1.5f;
		}
	}

	for (y=0;y<64;y++){
		for (x=0;x<64;x++){
			px=x-32;
			py=y-32;
/*			if(sqrt(px*px+py*py)<16){
				px=16-abs(px);
				py=16-abs(py);
				if(px<0)px=0;
				if(py<0)py=0;
			}*/
			float r=sqrt(px*px+py*py);
			if(r<16){
				texturedata[y*64+x]-=pow((1.0f-(r/16.0f)),0.5)+1.0f-(r/16.0f);
				if(texturedata[y*64+x]<0)texturedata[y*64+x]=0;
			}
		}
	}

	glTexImage2D(GL_TEXTURE_2D,0,1,64,64,0,GL_LUMINANCE,GL_FLOAT,texturedata);
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);
}

#ifndef FALSE
#define FALSE 0
#define TRUE 1
#endif

#ifdef WIN32
#define TEXFILE(x) "textures\\" x
#else
#define TEXFILE(x) "textures/" x
#endif

void LoadGLFont(){
		SDL_Surface *image[1];
		if(sets->mode256){
			image[0]=IMG_Load(TEXFILE("teksti_256.png"));
			printf("loading 256*256 font\n");
		}else{
			image[0]=IMG_Load(TEXFILE("teksti.png"));
			printf("loading 1024*1024 font\n");
		}
		glGenTextures(1, &texture[28]);		
		glBindTexture(GL_TEXTURE_2D, texture[28]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
}


int LoadGLTextures()		

{
	int Status=FALSE;					

	LoadGLFont();
//	AUX_RGBImageRec *TextureImage[1];	
//	memset(TextureImage,0,sizeof(void *)*1);


	Status=TRUE;						
		SDL_Surface *image[1];
		image[0]=NULL;
/*		image[0]=IMG_Load(TEXFILE("kuva.png"));
		//image[0]=IMG_Load("textures\\kuva.png");
		glGenTextures(1, &texture[0]);		
		glBindTexture(GL_TEXTURE_2D, texture[0]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);*/

/*		image[0]=IMG_Load(TEXFILE("alus1.png"));
		glGenTextures(1, &texture[1]);		
		glBindTexture(GL_TEXTURE_2D, texture[1]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
*/
		
		image[0]=IMG_Load(TEXFILE("tottero1.png"));
		glGenTextures(1, &texture[8]);		
		glBindTexture(GL_TEXTURE_2D, texture[8]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);


/*		image[0]=IMG_Load(TEXFILE("pallukka.png"));
		glGenTextures(1, &texture[3]);		
		glBindTexture(GL_TEXTURE_2D, texture[3]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
*/


		image[0]=IMG_Load(TEXFILE("pati0.png"));
		glGenTextures(1, &texture[4]);		
		glBindTexture(GL_TEXTURE_2D, texture[4]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);



		image[0]=IMG_Load(TEXFILE("pati1.png"));
		glGenTextures(1, &texture[5]);		
		glBindTexture(GL_TEXTURE_2D, texture[5]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		image[0]=IMG_Load(TEXFILE("jalki.png"));
		glGenTextures(1, &texture[6]);		
		glBindTexture(GL_TEXTURE_2D, texture[6]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);



/*		image[0]=IMG_Load(TEXFILE("tottero2.png"));
		glGenTextures(1, &texture[7]);		
		glBindTexture(GL_TEXTURE_2D, texture[7]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/
		/*
		image[0]=IMG_Load(TEXFILE("PLR2.png"));
		glGenTextures(1, &texture[8]);		
		glBindTexture(GL_TEXTURE_2D, texture[8]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);

*/
		
/*		image[0]=IMG_Load(TEXFILE("alus2_2.png"));
		glGenTextures(1, &texture[9]);		
		glBindTexture(GL_TEXTURE_2D, texture[9]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
*/


/*		image[0]=IMG_Load(TEXFILE("alus3.png"));
		glGenTextures(1, &texture[10]);		
		glBindTexture(GL_TEXTURE_2D, texture[10]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/


/*		image[0]=IMG_Load(TEXFILE("alus4.png"));
		glGenTextures(1, &texture[11]);		
		glBindTexture(GL_TEXTURE_2D, texture[11]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
*/


/*		image[0]=IMG_Load(TEXFILE("alus5.png"));
		glGenTextures(1, &texture[12]);		
		glBindTexture(GL_TEXTURE_2D, texture[12]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
*/


		image[0]=IMG_Load(TEXFILE("alus5_2.png"));
		glGenTextures(1, &texture[13]);		
		glBindTexture(GL_TEXTURE_2D, texture[13]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
	

		
		image[0]=IMG_Load(TEXFILE("sucker.png"));
		glGenTextures(1, &texture[14]);		
		glBindTexture(GL_TEXTURE_2D, texture[14]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		
/*		image[0]=IMG_Load(TEXFILE("alus6.png"));
		glGenTextures(1, &texture[15]);		
		glBindTexture(GL_TEXTURE_2D, texture[15]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
*/


		image[0]=IMG_Load(TEXFILE("pikku.png"));
		glGenTextures(1, &texture[16]);		
		glBindTexture(GL_TEXTURE_2D, texture[16]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);



		image[0]=IMG_Load(TEXFILE("pati6.png"));
		glGenTextures(1, &texture[17]);		
		glBindTexture(GL_TEXTURE_2D, texture[17]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);



		glGenTextures(1, &texture[18]);		
		generateLightFlare(&texture[18]);

		glGenTextures(1, &texture[15]);		
		generateLightNelio(&texture[15]);


		image[0]=IMG_Load(TEXFILE("pati20.png"));
		glGenTextures(1, &texture[19]);		
		glBindTexture(GL_TEXTURE_2D, texture[19]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		image[0]=IMG_Load(TEXFILE("Ohjus.png"));
		glGenTextures(1, &texture[20]);		
		glBindTexture(GL_TEXTURE_2D, texture[20]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		image[0]=IMG_Load(TEXFILE("pati8.png"));
		glGenTextures(1, &texture[21]);		
		glBindTexture(GL_TEXTURE_2D, texture[21]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		image[0]=IMG_Load(TEXFILE("muna.png"));
		glGenTextures(1, &texture[22]);		
		glBindTexture(GL_TEXTURE_2D, texture[22]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		
		image[0]=IMG_Load(TEXFILE("galax.png"));
		glGenTextures(1, &texture[23]);		
		glBindTexture(GL_TEXTURE_2D, texture[23]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		
/*		image[0]=IMG_Load(TEXFILE("alus10.png"));
		glGenTextures(1, &texture[24]);		
		glBindTexture(GL_TEXTURE_2D, texture[24]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);

*/
		
		
		image[0]=IMG_Load(TEXFILE("aukko.png"));
		glGenTextures(1, &texture[25]);		
		glBindTexture(GL_TEXTURE_2D, texture[25]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);



		image[0]=IMG_Load(TEXFILE("galax2.png"));
		glGenTextures(1, &texture[26]);		
		glBindTexture(GL_TEXTURE_2D, texture[26]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);

/*		image[0]=IMG_Load(TEXFILE("alus11.png"));
		glGenTextures(1, &texture[27]);		
		glBindTexture(GL_TEXTURE_2D, texture[27]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/
	
/*		image[0]=IMG_Load(TEXFILE("alus12.png"));
		glGenTextures(1, &texture[29]);		
		glBindTexture(GL_TEXTURE_2D, texture[29]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/		
/*		image[0]=IMG_Load(TEXFILE("alus12_2.png"));
		glGenTextures(1, &texture[30]);		
		glBindTexture(GL_TEXTURE_2D, texture[30]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);*/
		
/*		image[0]=IMG_Load(TEXFILE("alus13.png"));
		glGenTextures(1, &texture[31]);		
		glBindTexture(GL_TEXTURE_2D, texture[31]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);
*/
		
/*		image[0]=IMG_Load(TEXFILE("alus15.png"));
		glGenTextures(1, &texture[32]);		
		glBindTexture(GL_TEXTURE_2D, texture[32]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/
		
		image[0]=IMG_Load(TEXFILE("pati15.png"));
		glGenTextures(1, &texture[33]);		
		glBindTexture(GL_TEXTURE_2D, texture[33]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
//		free(image[0]->pixels);	
//		free(image[0]);			
		SDL_FreeSurface(image[0]);


		
		image[0]=IMG_Load(TEXFILE("alus20.png"));
		glGenTextures(1, &texture[34]);		
		glBindTexture(GL_TEXTURE_2D, texture[34]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);

		
/*		image[0]=IMG_Load(TEXFILE("alus16_2.png"));
		glGenTextures(1, &texture[35]);		
		glBindTexture(GL_TEXTURE_2D, texture[35]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/
		
/*		image[0]=IMG_Load(TEXFILE("pati16.png"));
		glGenTextures(1, &texture[36]);		
		glBindTexture(GL_TEXTURE_2D, texture[36]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/
		
/*		image[0]=IMG_Load(TEXFILE("alus18.png"));
		glGenTextures(1, &texture[37]);		
		glBindTexture(GL_TEXTURE_2D, texture[37]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/
		
		image[0]=IMG_Load(TEXFILE("epati18.png"));
		glGenTextures(1, &texture[38]);		
		glBindTexture(GL_TEXTURE_2D, texture[38]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);


		image[0]=IMG_Load(TEXFILE("meteor2.png"));
		glGenTextures(1, &texture[39]);		
		glBindTexture(GL_TEXTURE_2D, texture[39]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
		

		image[0]=IMG_Load(TEXFILE("electric.png"));
		glGenTextures(1, &texture[40]);		
		glBindTexture(GL_TEXTURE_2D, texture[40]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);


		image[0]=IMG_Load(TEXFILE("epati20.png"));
		glGenTextures(1, &texture[41]);		
		glBindTexture(GL_TEXTURE_2D, texture[41]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);

		image[0]=IMG_Load(TEXFILE("pati22.png"));
		glGenTextures(1, &texture[2]);		
		glBindTexture(GL_TEXTURE_2D, texture[2]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);


/*		image[0]=IMG_Load(TEXFILE("PLR3.png"));
		glGenTextures(1, &texture[10]);		
		glBindTexture(GL_TEXTURE_2D, texture[10]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);

		
		image[0]=IMG_Load(TEXFILE("PLR4.png"));
		glGenTextures(1, &texture[27]);		
		glBindTexture(GL_TEXTURE_2D, texture[27]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
*/

/*		image[0]=IMG_Load(TEXFILE("PLR4.png"));
		glGenTextures(1, &texture[42]);		
		glBindTexture(GL_TEXTURE_2D, texture[42]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);*/

/*		image[0]=IMG_Load(TEXFILE("teksti2.png"));
		glGenTextures(1, &texture[10]);		
		glBindTexture(GL_TEXTURE_2D, texture[10]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
	*/	
/*		image[0]=IMG_Load(TEXFILE("tausta1.png"));
		glGenTextures(1, &texture[27]);		
		glBindTexture(GL_TEXTURE_2D, texture[27]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);
	*/	
		
		if(sets->mode256){
		  image[0]=IMG_Load(TEXFILE("tausta2_256.png"));
		}else{
		  image[0]=IMG_Load(TEXFILE("tausta2.png"));
		}
		glGenTextures(1, &texture[29]);		
		glBindTexture(GL_TEXTURE_2D, texture[29]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);


		if(sets->mode256){
		  image[0]=IMG_Load(TEXFILE("tausta3_256.png"));
		}else{
		  image[0]=IMG_Load(TEXFILE("tausta3.png"));
		}
		glGenTextures(1, &texture[30]);		
		glBindTexture(GL_TEXTURE_2D, texture[30]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);

		if(sets->mode256){
			image[0]=IMG_Load(TEXFILE("alkukuva_256.png"));
		}else{
			image[0]=IMG_Load(TEXFILE("alkukuva.png"));
		}
		glGenTextures(1, &texture[31]);		
		glBindTexture(GL_TEXTURE_2D, texture[31]);
 		glTexImage2D(GL_TEXTURE_2D, 0, 3, image[0]->w, image[0]->h, 0, GL_RGB, GL_UNSIGNED_BYTE, image[0]->pixels);
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);	
		glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);	
		SDL_FreeSurface(image[0]);

		glGenTextures(1, &texture[42]);		
		generateTeamJuttu(&texture[42]);
		tiimitexture=texture[42];
		//seuraava vapaa 0,1,3,7,9,10,11,12,15,24,43, 32,36,37,35
		
//		alus[0].texture=texture[1];
//		alus[0].texture2=texture[9];
		alus[0].patitexture=texture[4];

		alus[1].patitexture=texture[5];
		alus[1].epatitexture=texture[4];

		alus[2].patitexture=texture[4];

//		alus[3].texture=texture[11];
		alus[3].patitexture=texture[4];
		alus[3].epatitexture=texture[4];
		
//		alus[4].texture=texture[12];
		alus[4].patitexture=texture[4];
		alus[4].epatitexture=texture[14];
		alus[4].texture2=texture[13];

//		alus[5].texture=texture[15];
		alus[5].patitexture=texture[17];
		alus[5].epatitexture=texture[16];
		alus[5].texture2=texture[16];

//		alus[6].texture=texture[19];

//		alus[7].texture=texture[2];
		alus[7].patitexture=texture[20];

//		alus[8].texture=texture[19];
		alus[8].patitexture=texture[21];
		alus[8].epatitexture=texture[22];

//		alus[9].texture=texture[24];
		alus[9].epatitexture=texture[25];

//		alus[10].texture=texture[27];
		alus[10].patitexture=texture[6];

//		alus[11].texture=texture[29];
//		alus[11].texture2=texture[30];
		alus[11].patitexture=texture[33];

//		alus[12].texture=texture[31];
		alus[12].patitexture=texture[18];

		//alus[13].texture=texture[24];
		alus[13].patitexture=texture[18];
		alus[13].epatitexture=texture[18];

//		alus[14].texture=texture[32];
		alus[14].patitexture=texture[33];

		alus[15].texture=texture[8];
		alus[15].patitexture=texture[36];
		//alus[15].texture2=texture[8];

		alus[16].patitexture=texture[18];
		alus[16].epatitexture=texture[18];

		
//		alus[17].texture=texture[37];
		alus[17].patitexture=texture[5];
		alus[17].epatitexture=texture[38];

		alus[18].texture2=texture[40];

		alus[19].patitexture=texture[18];
		
		alus[20].patitexture=texture[19];
		alus[20].epatitexture=texture[41];
		alus[20].texture=texture[34];

		alus[21].patitexture=texture[18];
		alus[21].epatitexture=texture[18];

		alus[22].patitexture=texture[2];
		alus[22].epatitexture=texture[18];

		alus[23].epatitexture=texture[2];
		alus[23].patitexture=texture[4];

		printf("moi!");
		return Status;						

	}


GLvoid ReSizeGLScene(GLsizei width, GLsizei height)				// Resize And Initialize The GL Window
{
	if (height==0)								// Prevent A Divide By Zero By
	{
		height=1;							// Making Height Equal One
	}
	glViewport(0, 0, width, height);					// Reset The Current Viewport
	glMatrixMode(GL_PROJECTION);						// Select The Projection Matrix
	glLoadIdentity();							// Reset The Projection Matrix
	gluPerspective(45.0f,(GLfloat)width/(GLfloat)height,1.0f,1024.0f);
	glMatrixMode(GL_MODELVIEW);						// Select The Modelview Matrix
	glLoadIdentity();							// Reset The Modelview Matrix
	LoadGLTextures();
	setLists();
}





void setupOpengl(int width, int height){

	//float ratio=(float)width/height;

	// GL-settings
	glCullFace(GL_BACK);
	glFrontFace(GL_CCW);
	//glEnable(GL_CULL_FACE);
	glEnable(GL_NORMALIZE);
	glEnable(GL_LINE_SMOOTH);
	glEnable(GL_TEXTURE_2D);
	glDepthFunc(GL_LEQUAL);				

	glEnable(GL_LIGHTING);
	glEnable(GL_COLOR_MATERIAL);
	glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER,GL_TRUE);
	glLightModeli(GL_LIGHT_MODEL_TWO_SIDE,GL_FALSE);
  glClearColor(0.0,0.0,0.0,1.0);

	// Ambient
	GLfloat lightambient[4]= {0.35, 0.35, 0.35, 1.0};
	glLightModelfv(GL_LIGHT_MODEL_AMBIENT,lightambient);

	// Mainlight
	{
		glEnable(GL_LIGHT0);

		GLfloat lightdiffuse[4]= {1.0, 1.0, 1.0, 1.0};
		glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);

		GLfloat lightspecular[4]={1.0, 1.0, 1.0, 1.0};
		glLightfv(GL_LIGHT0,GL_SPECULAR,lightspecular);
	}

/*

GLfloat lightambient[4]= {0.0, 0.0, 0.0, 1.0};
GLfloat lightdiffuse[4]= {0.0, 1.0, 1.0, 1.0};
GLfloat lightspecular[4]={1.0, 1.0, 1.0, 1.0};
GLfloat lightspecular[4]={0, 0, 0, 0.0};

////GLfloat lightambient[4]= { 0.1,0.1,0.1,1.0};
////GLfloat lightdiffuse[4]= { 1.0,1.0,1.0,1.0};
////GLfloat lightspecular[4]={ 1.0,1.0,1.0,1.0};
//glLightModelfv(GL_LIGHT_MODEL_AMBIENT,lightambient);
//glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);
//glLightfv(GL_LIGHT0,GL_SPECULAR,lightspecular);
//glLightf(GL_LIGHT0,GL_QUADRATIC_ATTENUATION,0.01);
//glLightf(GL_LIGHT0, GL_CONSTANT_ATTENUATION, 1);
glEnable(GL_LIGHT0);

glLightfv(GL_LIGHT1,GL_DIFFUSE, lightdiffuse);
glLightfv(GL_LIGHT1,GL_SPECULAR,lightspecular);
//glEnable(GL_LIGHT1);

glLightfv(GL_LIGHT2,GL_DIFFUSE, lightdiffuse);
glLightfv(GL_LIGHT2,GL_SPECULAR,lightspecular);
//glEnable(GL_LIGHT2);
glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER,GL_TRUE);
glLightModeli(GL_LIGHT_MODEL_TWO_SIDE,GL_FALSE);
//GLfloat specular[3]={1,1,1};
GLfloat specular[3]={0,0,0};
//glMaterialfv(GL_FRONT,GL_SPECULAR,specular);
//glMaterialf(GL_FRONT,GL_SHININESS,120.0);
GLfloat zero[4]={0,0,0,0};
//glMaterialfv(GL_BACK,GL_AMBIENT,zero);
//glMaterialfv(GL_BACK,GL_DIFFUSE,zero);
//glMaterialfv(GL_BACK,GL_SPECULAR,zero);

*/

//	glClearColor(0,0,0,0);

//	glViewport(0,0,width,height);

//	glMatrixMode(GL_PROJECTION);
//	glLoadIdentity();

	//glOrtho(-1.0*4/3,1.0*4/3,-1,1,0,1);
	//glOrtho(-1.0*4/3,1.0*4/3,-1,1,0,1);

//	gluPerspective(45.0,ratio,1.0,1024.0);
	ReSizeGLScene(width, height);

}



