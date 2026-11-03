/*
 * $Id: audio.h,v 1.12 2003/04/17 20:27:06 tgo Exp $
 *
 *
 * $Log: audio.h,v $
 * Revision 1.12  2003/04/17 20:27:06  tgo
 * no message
 *
 * Revision 1.11  2003/01/06 23:03:16  tgo
 * no message
 *
 * Revision 1.10  2002/05/28 14:33:56  hipo
 * Juttu.
 *
 * Revision 1.9  2002/05/28 07:34:36  tgo
 * no message
 *
 * Revision 1.8  2002/05/26 13:59:01  tgo
 * no message
 *
 * Revision 1.7  2002/05/25 13:20:09  hipo
 * boolean -> bool
 *
 * Revision 1.6  2002/05/25 12:35:19  msell
 * no message
 *
 * Revision 1.5  2002/05/25 12:19:57  hipo
 * Linux päivitys.
 *
 * Revision 1.4  2002/05/24 17:36:13  msell
 * AUDIO_NONE kääntyy
 *
 * Revision 1.6  2002/05/19 13:40:19  msell
 * Säätöä
 *
 * Revision 1.5  2002/05/18 12:29:35  msell
 * Valot ja äänijärjestelmän parantelua
 *
 * Revision 1.4  2002/05/16 18:41:16  msell
 * Vektorifunctioita ja kamera
 *
 * Revision 1.3  2002/05/15 21:07:23  msell
 * Äänisäätöä
 *
 * Revision 1.2  2002/05/15 15:21:00  msell
 * Äänisäätöä
 *
 * Revision 1.1  2002/05/15 14:36:39  msell
 * Yksinkertainen SDL-runko (Kääntyy toistaiseksi vain windowsissa).
 * Mukana myös musiikinsoitto fmodilla.
 *
 *
 *
 * $Date: 2003/04/17 20:27:06 $
 *
 */

#ifndef __AUDIO_H_INCLUDED__
#define __AUDIO_H_INCLUDED__


//#if 1
#define AUDIO_FMOD
//#else
//#define AUDIO_NONE
//#endif
//Only FMOD and NONE is supported, choose one




#ifdef AUDIO_FMOD
#include "fmod.h"
#endif


class Sound;

typedef void(* STOPCALLBACK)(Sound *sound);

#define SOUNDTYPE_AUTODETECT 0
#define SOUNDTYPE_MODULE 1
#define SOUNDTYPE_STREAM 2
#define SOUNDTYPE_SAMPLE 3

class Sound{
private:
#ifdef AUDIO_FMOD
	int type;
	FMUSIC_MODULE *module;
	FSOUND_STREAM *stream;
	FSOUND_SAMPLE *sample;
	int channel;
#endif
	bool loops;
	bool finished;
	bool running;
	float volume;
	STOPCALLBACK stopcallback;
	bool isFinished(void);

public:
	Sound(Sound *source);
	Sound(char *filename);
	Sound(char *filename, int type);
	Sound(char *filename, bool loops);
	Sound(char *filename, int type, bool loops);
	void load(char *filename, int type, bool loops);
	void play();
	void stop();
	void setStopCallback(STOPCALLBACK callback);
	void setVolume(float volume);
	
	//Do not use methods below
	void setFinished(void);
	void update(void);
};

void initAudio(void);
void uninitAudio(void);
void updateAudio(void);
extern char working;

#endif
