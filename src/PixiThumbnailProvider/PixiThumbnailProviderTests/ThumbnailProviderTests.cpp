#include "pch.h"
#include "CppUnitTest.h"
#include "../PixiThumbnailProvider/PngImageProvider.h"
#include <iostream>
#include <vector>
#include <memory>
#pragma comment(lib, "gdiplus.lib")

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ThumbnailProviderTests
{
  TEST_CLASS(ThumbnailProviderTests)
  {
  public:

    static void GetEnvTempPath(std::wstring& input_parameter) {
      wchar_t* env_var_buffer = nullptr;
      std::size_t size = 0;
      if (_wdupenv_s(&env_var_buffer, &size, L"TEMP") == 0 &&
        env_var_buffer != nullptr) {
        input_parameter = static_cast<const wchar_t*>(env_var_buffer);
      }
    }

    TEST_METHOD(ExtractPngFromPixi)
    {
      std::wstring input_parameter;
      GetEnvTempPath(input_parameter);
      auto pixiFile = "images\\p1.pixi";
      
      PngImageProvider pngBitmapProvider([](std::wstring line) {
        std::wcout << line;
        });

      ULONG_PTR gdiplusToken1;
      auto imageHandle = pngBitmapProvider.LoadPixiBitmapPreviewHandle(pixiFile, gdiplusToken1);
      pngBitmapProvider.Save(L"images\\fromPixiFileAsHandle.png", imageHandle);

      ULONG_PTR gdiplusToken;
      auto image = pngBitmapProvider.LoadPixiBitmapPreview(pixiFile, gdiplusToken);
      Assert::IsNotNull(image);
      pngBitmapProvider.Save(L"images\\fromPixiFile.png", image);

      //save to a file
      //SaveBytesAsImage(bytes);
    }
    void SaveBytesAsImage()
    {
      PngImageProvider pngBitmapProvider([](std::wstring line) {
        std::wcout << line;
        });
      auto pixiFile = "images\\p1.pixi";
      auto bytes = pngBitmapProvider.LoadPixiPreviewBytes(pixiFile);
      std::ofstream outfile("images\\p1.png", std::ios::out | std::ios::binary);
      outfile.write((const char*)&bytes[0], bytes.size());
    }
  };
}
