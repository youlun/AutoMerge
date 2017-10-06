# AutoMerge
自动调用mkvmerge/l-smash muxer 合并.hevc .flac .m4a ....

# 一条龙服务示例
1. 用 [EACExtract](https://github.com/youlun/EACExtract) 自动提取轨道
2. 手动删除重复音轨，将副音轨转换为aac等
3. 用 [ChapterTool](https://github.com/tautcony/ChapterTool/) 提取章节
4. 编辑章节名，将章节文件改名为 000xx.txt，移动到 m2ts 相同目录中
5. 压制
6. 用本工具自动封装
7. 播放并检查后上传

![automerge.jpg](https://i.loli.net/2017/09/27/59cb73002e26c.jpg)

# 目录结构示例
<pre>
------------------
|-AutoMerge.exe
|-EACExtract.exe
|-encode.bat
|--Vol.1
|---00001.vpy
|---00001.m2ts
|---00001.m2ts.lwi
|---00001.2.flac
|---00001.3.m4a
|---00001.4.sup
|---00001.txt
|---00001.hevc
|--Vol.2
|---....
|--completed
|----Vol.1
|------00001 [A1B2C3D4].mkv
|----Vol.2
|------....
------------------
</pre>
