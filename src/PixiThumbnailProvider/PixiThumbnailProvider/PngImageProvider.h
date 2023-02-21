#pragma once
#include <shlwapi.h>
#include <wincodec.h>
#include <string>
#include <functional>
#include <gdiplus.h>
#include <stdexcept>
#include "Utils.h"

#pragma comment(lib, "gdiplus.lib")
#pragma comment(lib, "windowscodecs.lib")

#include <iostream>
#include <objidl.h>
#include <gdiplus.h>
#pragma warning(disable : 4996)
#pragma warning(disable : 4458)

class PngImageProvider
{
  std::function<void(std::wstring)> m_logger;

  HBITMAP HandleFromBitmap(Gdiplus::Bitmap* bmp)
  {
    HBITMAP hBitmap = nullptr;
    bmp->GetHBITMAP(Gdiplus::Color(0xFFFFFFFF), &hBitmap);
    return hBitmap;
  }

public:
  PngImageProvider(std::function<void(std::wstring)> logger)
  {
    m_logger = logger;
  }

  void Save(std::wstring filePath, HBITMAP bitmap)
  {
    //auto palette = (HPALETTE)GetStockObject(DEFAULT_PALETTE);
    auto palette = (HPALETTE)0;
    auto bmp = Gdiplus::Bitmap::FromHBITMAP(bitmap, palette);
    Save(filePath, bmp);
  }

  void Save(std::wstring filePath, Gdiplus::Bitmap* bmp)
  {
    CLSID pngClsid;
    CLSIDFromString(L"{557CF406-1A04-11D3-9A73-0000F81EF32E}", &pngClsid);
    bmp->Save(filePath.c_str(), &pngClsid, NULL);
  }

  template<typename T>
  IStream* StreamFromBytes(std::vector<T> bytes)
  {
    return SHCreateMemStream(&bytes[0], (UINT)bytes.size());;
  }

  template<typename T>
  HBITMAP LoadPngHandle(std::vector<T> bytes, ULONG_PTR& gdiplusToken)
  {
    auto stream = StreamFromBytes(bytes);
    return LoadPngHandle(stream, gdiplusToken);
  }

  template<typename T>
  Gdiplus::Bitmap* LoadPngImage(std::vector<T> bytes, ULONG_PTR& gdiplusToken)
  {
    auto stream = StreamFromBytes(bytes);
    return LoadPngImage(stream, gdiplusToken);
  }

  std::vector<unsigned char> LoadPixiPreviewBytes(std::string pixiFile)
  {
    std::wstring pixiFileW(pixiFile.begin(), pixiFile.end());
    return LoadPixiPreviewBytes(pixiFileW);
  }

  std::vector<unsigned char> LoadPixiPreviewBytes(std::wstring pixiFile)
  {
    auto streamPos = 0;

    load_bytes<unsigned char>(pixiFile, streamPos, 21);//header
    streamPos += 21;
    auto previewHeader = load_bytes<unsigned char>(pixiFile, 21, 4);
    streamPos += 4;

    const auto previewSize = BytesToInt32(previewHeader, 0, true);

    auto bytes = load_bytes<unsigned char>(pixiFile, streamPos, previewSize);
    return bytes;
  }

  HBITMAP LoadPixiBitmapPreviewHandle(std::string pixiFilePath, ULONG_PTR& gdiplusToken)
  {
    auto bytes = LoadPixiPreviewBytes(pixiFilePath);
    return LoadPngHandle(bytes, gdiplusToken);
  }

  HBITMAP LoadPixiBitmapPreviewHandle(std::wstring pixiFilePath, ULONG_PTR& gdiplusToken)
  {
    auto bytes = LoadPixiPreviewBytes(pixiFilePath);
    return LoadPngHandle(bytes, gdiplusToken);
  }

  Gdiplus::Bitmap* LoadPixiBitmapPreview(std::string pixiFilePath, ULONG_PTR& gdiplusToken)
  {
    auto bytes = LoadPixiPreviewBytes(pixiFilePath);
    return LoadPngImage(bytes, gdiplusToken);
  }

private:

  Gdiplus::Bitmap* PngImageProvider::LoadPngImage(IStream* stream, ULONG_PTR& gdiplusToken)
  {
    Gdiplus::GdiplusStartupInput gdiplusStartupInput;
    Gdiplus::GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);
    auto bmp = Gdiplus::Bitmap::FromStream(stream);
    if (!bmp)
    {
      m_logger(L" Unable to open image file with Gdiplus.");
      return nullptr;
    }
    //Gdiplus::GdiplusShutdown(gdiplusToken);will make bmp* corrupted

    return bmp;
  }
    

  HBITMAP LoadPngHandle(IStream* stream, ULONG_PTR& gdiplusToken)
  {
    auto bmp = LoadPngImage(stream, gdiplusToken);
    if (!bmp)
    {
      m_logger(L" Unable to open image file with Gdiplus.");
      return nullptr;
    }
    auto hBitmap = HandleFromBitmap(bmp);
    //Gdiplus::GdiplusShutdown(gdiplusToken); //would corrupt handle
    return hBitmap;
  }

};

