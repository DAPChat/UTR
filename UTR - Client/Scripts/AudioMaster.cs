using Godot;
using System;

public partial class AudioMaster : AudioStreamPlayer
{
	static AudioStreamPlayer pl;

	public override void _Ready()
	{
		pl = this;
	}


	public static void Play(string path)
	{
		if (pl == null) return;

		pl.Stream = ResourceLoader.Load<AudioStream>(path);
		pl.Play();
	}
}
