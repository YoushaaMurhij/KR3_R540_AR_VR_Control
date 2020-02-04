# KR3_R540_AR_VR_Control

This package is based on HTC VIVE Pro instruments to simulate and control  KR3_R540 robot from Unity Environment. 
                                            ![GitHub KUKA KR3](/3d-kuka.png)

# Introduction
This work is based on my TECIS 2019 paper "An application to simulate and control industrial robot in virtual reality environment integrated with IR stereo camera sensor". 
The main goal of this research is to test for potential ways to control KUKA KR10 industrial arm manipulator using Virtual Reality technology and check for the advantages of applying this control methods. The final version of this application aims to achieve this goal by establishing an interaction between the user and the manipulator inside a virtual environment developed using the game engine Unity3D and the HTC VIVE Pro headset for the virtual visualization. By applying this control method, the user does not have to operate on site and instead he can work remotely. In addition to the ability to use online programming of the manipulator. The application is designed to simplify the controlling ways by displaying a complete virtual environment where the tridimensional model of the robotic arm can be visualized and programmed according to the real manipulator's parameters and specifications. All the movements and parameters in the virtual environment can be synchronized with the real robot in an on-line or online path planning depending on the application or the task.The system integrates a set of virtual reality controllers and Leapmotion sensor as options to allow the user to control and see the robot and its parameters in the virtual environment. As a result of this research, the manipulator moves on the planned trajectory in a smooth way after applying some filtering techniques without losing its accuracy.

Keywords: Virtual Reality; Robot; KUKA; Unity3D; HTC VIVE, Leapmotion.

### Citation
If you find our work useful in your research, please consider citing:

        @article{MURHIJ2019203,
            title = "An application to simulate and control industrial robot in virtual reality environment integrated with IR stereo camera sensor",
            journal = "IFAC-PapersOnLine",
            year = "2019",
            note = "19th IFAC Conference on Technology, Culture and International Stability TECIS 2019",
            issn = "2405-8963",
            doi = "https://doi.org/10.1016/j.ifacol.2019.12.473",
            author = "Youshaa Murhij and Vladimir Serebrenny",
        }

# Installation & usage
* Install Unity3D >= v1.5f following official instruction.
* Clone this repo.
* Install dependencies in Unity (StearmVR and Vive)
* Open the project and connect VR instuments.
* Enjoy the new 3D virtual world
