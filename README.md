# What

This is an example of using models trained with TensorFlow or ONNX in Unity application for image classification and object detection. It uses [Barracuda inference engine](https://docs.unity3d.com/Packages/com.unity.barracuda@0.4/manual/index.html) - please note that Barracuda is still in development preview and changes frequently.

More details in my [blogpost](https://classifai.net/blog/tensorflow-onnx-unity/).

Classify results:

![](https://raw.githubusercontent.com/Syn-McJ/TFClassify-Unity-Barracuda/master/SampleImages/classify1.png)
![](https://raw.githubusercontent.com/Syn-McJ/TFClassify-Unity-Barracuda/master/SampleImages/classify2.png)

Detect results:

![](https://raw.githubusercontent.com/Syn-McJ/TFClassify-Unity-Barracuda/master/SampleImages/detect1.png)
![](https://raw.githubusercontent.com/Syn-McJ/TFClassify-Unity-Barracuda/master/SampleImages/detect2.png)

If you're looking for similar example but using TensorflowSharp plugin instead of Barracuda, see my [previous repo](https://github.com/Syn-McJ/TFClassify-Unity).

# How

You'll need Unity 2019.3 or above. Versions 2019.2.x seem to have a bug with WebCamTexture and Vulkan that causes memory leak.

- Open the project in Unity.
- Install Barracuda 0.4.0-preview plugin from `Window -> Package Maanger` (the sample didn't work with 0.5.0-preview last time I checked it).
- In `Edit -> Player Settings -> Other settings` make sure that you have Vulkan in Graphics APIs for Android or Metal for iOS (remove Auto Graphics API check if neccessary). Barracuda is also suppose to work with OpenGLES3 + ES3.1, but I didn't have any luck with it.
- Open Classify or Detect scene in Assets folder.
- Make sure that Classifier or Detector object has Model file and Labels file set.
- in `File -> Build settings` choose one of the scenes and hit `Build and run`.

For iOS, you might need to fix team settings and privacy request message for camera in Xcode.

Barracuda repository might be found [here](https://github.com/Unity-Technologies/barracuda-release).

# How to use your own model

There are limited range of neural network architectures that I managed to get to run with Barracuda. [Read my blogpost](https://classifai.net/blog/tensorflow-onnx-unity/) to see what's working and what isn't.

# Notes

I'm not a Unity expert, so if you found any problems with this example feel free to open an issue.
