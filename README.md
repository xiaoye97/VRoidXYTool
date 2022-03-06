# VRoidXYTool

[English Readme](README_English.md)

用于VRoidStudio的扩展插件

## 免费声明
本插件为完全免费插件，禁止任何形式的售卖，近日看到有淘宝无良卖家在贩卖VRoidStudio软件本体和汉化插件，请大家注意警惕，如果看到请帮忙举报一下，谢谢。

## 下载
[最新版本][4]

## 简介

- 基于[BeplnEx][1]
- 链接纹理功能，通过外部绘图工具(PS/SAI等)对纹理进行处理，并实时同步到VRoid Studio内
- 镜头位置预设，快速定位到全身和脸部的四方向视角，可选透视或正交模式，支持自定义位置
- 参考工具，可以添加标尺和参考图
- 姿势预设，可以在摄影棚中的姿势模式中，保存和加载自定义的姿势预设
- 抗锯齿，除了在摄影棚中可以抗锯齿之外，在编辑模式下也可以开启抗锯齿
- MMD播放器(实验性)，可以在摄影棚中导入VMD动画文件进行播放
- 视频录制，在VRoid内进行视频录制高清视频

![图示](https://cdn.jsdelivr.net/gh/xiaoye97/VRoidXYTool@master/LinkTexturePreview.gif)

![图示](MMDPreview.gif)

## 教程

- 视频教程见[B站][2]

## Q&A

`Q:` 我没有BepInEx文件夹怎么办？

`A:` 到BepInEx仓库下载releases进行安装，或者安装VRoid[汉化插件][3]，汉化插件的安装包内附带了BepInEx

`Q:` 我安装了插件，在软件内怎么打开？

`A:` 默认开启快捷键为Tab，可以在配置文件中修改

`Q:` 链接纹理怎么用？

`A:` 在软件内编辑任意纹理，都会在插件中显示当前编辑的纹理，然后点击导出纹理，然后用外部绘图工具打开导出的纹理进行修改，修改之后，保存就可以同步到软件内。

`Q:` 链接纹理默认的文件夹在软件安装目录，我想换个目录怎么办？

`A:` 修改配置文件，可以自定义链接目录，链接目录的配置留空则为默认路径

`Q:` 我遇到了bug或者有功能建议怎么反馈？

`A:` bug提交Issues或到VRoid交流群(684544577)或者我的私人小群(528385469)内找我，提建议到群内找我


[1]: https://github.com/BepInEx/BepInEx/releases
[2]: https://www.bilibili.com/video/BV1TP4y1V7Qn/
[3]: https://www.bilibili.com/video/BV1BL41137Tc/
[4]: https://github.com/xiaoye97/VRoidXYTool/releases