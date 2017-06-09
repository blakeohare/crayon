﻿using System;
using OpenTK.Graphics.OpenGL;

// TODO: remove some of the generated code-isms such as excessive parenthesis.

namespace Interpreter.Libraries.Game
{
    public static class OpenTkRenderer
    {
        private class GlRenderState
        {
            public GlRenderState(int mode, int textureId, int r, int g, int b, int a)
            {
                this.mode = mode;
                this.textureId = textureId;
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            public int mode;
            public int textureId;
            public int r;
            public int g;
            public int b;
            public int a;
        }

        public static void render(int[] commands, int length, object[][] texturesNativeData, int VW, int VH, int RW, int RH)
        {
            bool scaled = VW != RW || VH != RH;
            bool rotated = false;
            int i = 0;
            int image_i = 0;
            int width = 0;
            int height = 0;
            int cropWidth;
            int cropHeight;
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            int lineWidth = 0;
            int j = 0;
            int ptCount = 0;
            int startX = 0;
            int startY = 0;
            int endX = 0;
            int endY = 0;
            int mask = 0;
            double textureWidth;
            double textureHeight;
			double textureResourceWidth;
			double textureResourceHeight;
            double textureLeft;
            double textureRight;
            double textureTop;
            double textureBottom;
			double croppedLeft;
			double croppedRight;
			double croppedTop;
			double croppedBottom;
            double slope = 0.0;
            double slopeScalingCoefficient = 0.0;
            double offsetXComp = 0.0;
            double offsetYComp = 0.0;
            double ax = 0.0;
            double ay = 0.0;
            double bx = 0.0;
            double by = 0.0;
            double cx = 0.0;
            double cy = 0.0;
            double dx = 0.0;
            double dy = 0.0;
            double tx = 0.0;
            double ty = 0.0;
            double angle = 0.0;
            double r_cos = 0.0;
            double r_sin = 0.0;
            int red = 0;
            int green = 0;
            int blue = 0;
            int alpha = 0;
            int textureId = (-1);
            object[] textureNativeData;
            object[] textureResourceNativeData;
            GlRenderState state = null;
            int[] textureArray = new int[1];
            while (i < length)
            {
                switch (commands[i])
                {
                    case 1:
                        left = commands[i | 1];
                        top = commands[i | 2];
                        right = commands[i | 3] + left;
                        bottom = commands[i | 4] + top;
                        red = commands[i | 5];
                        green = commands[i | 6];
                        blue = commands[i | 7];
                        alpha = commands[i | 8];

                        if (red > 255 || red < 0) red = red > 255 ? 255 : 0;
                        if (green > 255 || green < 0) green = green > 255 ? 255 : 0;
                        if (blue > 255 || blue < 0) blue = blue > 255 ? 255 : 0;
                        if (alpha > 255 || alpha < 0) alpha = alpha > 255 ? 255 : 0;

                        if (right >= 0 && left < VW && bottom > 0 && top < VH)
                        {
                            if (state == null || state.mode != 1 || state.textureId != 0 || state.r != red || state.g != green || state.b != blue || state.a != alpha)
                            {
                                if (state == null)
                                {
                                    state = new GlRenderState(0, 0, 0, 0, 0, 0);
                                }
                                else
                                {
                                    GL.End();
                                }
                                state.mode = 1;
                                state.textureId = 0;
                                state.r = red;
                                state.g = green;
                                state.b = blue;
                                state.a = alpha;
                                GL.Disable(EnableCap.Texture2D);
                                GL.Color4((byte)red, (byte)green, (byte)blue, (byte)alpha);
                                GL.Begin(BeginMode.Quads);
                            }
                            if (scaled)
                            {
                                GL.Vertex2(left * RW / VW, top * RH / VH);
                                GL.Vertex2(right * RW / VW, top * RH / VH);
                                GL.Vertex2(right * RW / VW, bottom * RH / VH);
                                GL.Vertex2(left * RW / VW, bottom * RH / VH);
                            }
                            else
                            {
                                GL.Vertex2(left, top);
                                GL.Vertex2(right, top);
                                GL.Vertex2(right, bottom);
                                GL.Vertex2(left, bottom);
                            }
                        }
                        break;

                    case 2:
                        left = commands[i | 1];
                        top = commands[i | 2];
                        right = commands[i | 3] + left;
                        bottom = commands[i | 4] + top;
                        red = commands[i | 5];
                        green = commands[i | 6];
                        blue = commands[i | 7];
                        alpha = commands[i | 8];
                        width = (right - left);
                        height = (bottom - top);
                        startX = left + width / 2;
                        startY = top + height / 2;
                        width = width / 2;
                        height = height / 2;

                        if (red > 255 || red < 0) red = red > 255 ? 255 : 0;
                        if (green > 255 || green < 0) green = green > 255 ? 255 : 0;
                        if (blue > 255 || blue < 0) blue = blue > 255 ? 255 : 0;
                        if (alpha > 255 || alpha < 0) alpha = alpha > 255 ? 255 : 0;

                        if (right >= 0 && left < VW && bottom >= 0 && top < VH && width > 0 && height > 0)
                        {
                            if (state != null)
                            {
                                GL.End();
                                state = null;
                            }
                            GL.Disable(EnableCap.Texture2D);
                            GL.Color4((byte)red, (byte)green, (byte)blue, (byte)alpha);
                            GL.Begin(BeginMode.Polygon);
                            ptCount = (width + height) / 8;
                            if (ptCount < 16)
                            {
                                ptCount = 16;
                            }
                            j = 0;
                            while ((j < ptCount))
                            {
                                endX = (int)(Math.Cos(-6.28318530717958 * j / ptCount) * width + startX);
                                endY = (int)(Math.Sin(-6.28318530717958 * j / ptCount) * height + startY);
                                if (scaled)
                                {
                                    GL.Vertex2(endX * RW / VW, endY * RH / VH);
                                }
                                else
                                {
                                    GL.Vertex2(endX, endY);
                                }
                                j += 1;
                            }
                            GL.End();
                        }
                        break;

                    case 3:
                        startX = commands[i | 1];
                        startY = commands[i | 2];
                        endX = commands[i | 3];
                        endY = commands[i | 4];
                        lineWidth = commands[i | 5];
                        red = commands[i | 6];
                        green = commands[i | 7];
                        blue = commands[i | 8];
                        alpha = commands[i | 9];
                        left = startX;
                        right = startX;

                        if (red > 255 || red < 0) red = red > 255 ? 255 : 0;
                        if (green > 255 || green < 0) green = green > 255 ? 255 : 0;
                        if (blue > 255 || blue < 0) blue = blue > 255 ? 255 : 0;
                        if (alpha > 255 || alpha < 0) alpha = alpha > 255 ? 255 : 0;

                        if (startX > endX)
                        {
                            left = endX;
                        }
                        else
                        {
                            right = endX;
                        }
                        top = startY;
                        bottom = startY;
                        if (startY > endY)
                        {
                            top = endY;
                        }
                        else
                        {
                            bottom = endY;
                        }
                        if (startX > endX)
                        {
                            j = startX;
                            startX = endX;
                            endX = j;
                            j = startY;
                            startY = endY;
                            endY = j;
                        }
                        slope = -(1.0 * endY - startY) / (endX - startX);
                        textureRight = slope;
                        if ((textureRight < 0))
                        {
                            textureRight = -textureRight;
                        }
                        slopeScalingCoefficient = System.Math.Pow((1 + 1.0 / (slope * slope)), 0.5);
                        offsetXComp = lineWidth / slopeScalingCoefficient / 2;
                        offsetYComp = lineWidth / (2 * textureRight * slopeScalingCoefficient);
                        ax = startX;
                        ay = startY;
                        bx = endX;
                        by = endY;
                        cx = bx;
                        cy = by;
                        dx = ax;
                        dy = ay;
                        if (slope > 0)
                        {
                            ax -= offsetXComp;
                            ay -= offsetYComp;
                            bx -= offsetXComp;
                            by -= offsetYComp;
                            cx += offsetXComp;
                            cy += offsetYComp;
                            dx += offsetXComp;
                            dy += offsetYComp;
                        }
                        else
                        {
                            ax += offsetXComp;
                            ay -= offsetYComp;
                            bx += offsetXComp;
                            by -= offsetYComp;
                            cx -= offsetXComp;
                            cy += offsetYComp;
                            dx -= offsetXComp;
                            dy += offsetYComp;
                        }
                        if (state == null || state.mode != 1 || state.textureId != 0 || state.r != red || state.g != green || state.b != blue || state.a != alpha)
                        {
                            if (state == null)
                            {
                                state = new GlRenderState(0, 0, 0, 0, 0, 0);
                            }
                            else
                            {
                                GL.End();
                            }
                            state.mode = 1;
                            state.textureId = 0;
                            state.r = red;
                            state.g = green;
                            state.b = blue;
                            state.a = alpha;
                            GL.Disable(EnableCap.Texture2D);
                            GL.Color4((byte)red, (byte)green, (byte)blue, (byte)alpha);
                            GL.Begin(BeginMode.Quads);
                        }
                        if (scaled)
                        {
                            GL.Vertex2(ax * RW / VW, ay * RH / VH);
                            GL.Vertex2(bx * RW / VW, by * RH / VH);
                            GL.Vertex2(cx * RW / VW, cy * RH / VH);
                            GL.Vertex2(dx * RW / VW, dy * RH / VH);
                        }
                        else
                        {
                            GL.Vertex2(ax, ay);
                            GL.Vertex2(bx, by);
                            GL.Vertex2(cx, cy);
                            GL.Vertex2(dx, dy);
                        }
                        break;

                    case 4:
                        ax = commands[i | 1];
                        ay = commands[i | 2];
                        bx = commands[i | 3];
                        by = commands[i | 4];
                        cx = commands[i | 5];
                        cy = commands[i | 6];
                        red = commands[i | 7];
                        green = commands[i | 8];
                        blue = commands[i | 9];
                        alpha = commands[i | 10];

                        if (red > 255 || red < 0) red = red > 255 ? 255 : 0;
                        if (green > 255 || green < 0) green = green > 255 ? 255 : 0;
                        if (blue > 255 || blue < 0) blue = blue > 255 ? 255 : 0;
                        if (alpha > 255 || alpha < 0) alpha = alpha > 255 ? 255 : 0;

                        if (state != null)
                        {
                            GL.End();
                            state = null;
                        }
                        GL.Disable(EnableCap.Texture2D);
                        GL.Color4((byte)red, (byte)green, (byte)blue, (byte)alpha);
                        GL.Begin(BeginMode.Polygon);
                        ptCount = (width + height) / 8;
                        if (ptCount < 16)
                        {
                            ptCount = 16;
                        }
                        if (scaled)
                        {
                            GL.Vertex2(ax * RW / VW, ay * RH / VH);
                            GL.Vertex2(bx * RW / VW, by * RH / VH);
                            GL.Vertex2(cx * RW / VW, cy * RH / VH);
                        }
                        else
                        {
                            GL.Vertex2(ax, ay);
                            GL.Vertex2(bx, by);
                            GL.Vertex2(cx, cy);
                        }
                        GL.End();
                        break;

                    case 5:
                        ax = commands[i | 1];
                        ay = commands[i | 2];
                        bx = commands[i | 3];
                        by = commands[i | 4];
                        cx = commands[i | 5];
                        cy = commands[i | 6];
                        dx = commands[i | 7];
                        dy = commands[i | 8];
                        red = commands[i | 9];
                        green = commands[i | 10];
                        blue = commands[i | 11];
                        alpha = commands[i | 12];

                        if (red > 255 || red < 0) red = red > 255 ? 255 : 0;
                        if (green > 255 || green < 0) green = green > 255 ? 255 : 0;
                        if (blue > 255 || blue < 0) blue = blue > 255 ? 255 : 0;
                        if (alpha > 255 || alpha < 0) alpha = alpha > 255 ? 255 : 0;

                        if (state != null)
                        {
                            GL.End();
                            state = null;
                        }
                        GL.Disable(EnableCap.Texture2D);
                        GL.Color4((byte)red, (byte)green, (byte)blue, (byte)alpha);
                        GL.Begin(BeginMode.Polygon);
                        ptCount = (width + height) / 8;
                        if (ptCount < 16)
                        {
                            ptCount = 16;
                        }
                        if (scaled)
                        {
                            GL.Vertex2(ax * RW / VW, ay * RH / VH);
                            GL.Vertex2(bx * RW / VW, by * RH / VH);
                            GL.Vertex2(cx * RW / VW, cy * RH / VH);
                            GL.Vertex2(dx * RW / VW, dy * RH / VH);
                        }
                        else
                        {
                            GL.Vertex2(ax, ay);
                            GL.Vertex2(bx, by);
                            GL.Vertex2(cx, cy);
                            GL.Vertex2(dx, dy);
                        }
                        GL.End();
                        break;

                    case 6:
                        textureNativeData = texturesNativeData[image_i++];
						textureResourceNativeData = (object[])textureNativeData[0];

                        if (!(bool)textureResourceNativeData[1])
                        {
                            if (state != null)
                            {
                                state = null;
                                GL.End();
                            }
                            textureResourceNativeData[2] = GlUtil.ForceLoadTexture((UniversalBitmap)textureResourceNativeData[3]);
                            textureResourceNativeData[1] = true;
                        }

                        mask = commands[i | 1];
                        rotated = (mask & 4) != 0;
                        textureId = (int)textureResourceNativeData[2];
                        alpha = 255;
                        startX = commands[i | 8]; // left
                        startY = commands[i | 9]; // top

                        // alpha
                        if ((mask & 8) != 0)
                        {
                            alpha = commands[i | 11];
                            if (alpha < 0 || alpha > 255) alpha = alpha < 0 ? 0 : 255;
                        }

						textureLeft = (double)textureNativeData[1];
						textureTop = (double)textureNativeData[2];
						textureRight = (double)textureNativeData[3];
						textureBottom = (double)textureNativeData[4];
						width = (int)textureNativeData[5];
						height = (int)textureNativeData[6];

                        textureWidth = textureRight - textureLeft;
                        textureHeight = textureBottom - textureTop;
						textureResourceWidth = (int)textureResourceNativeData[4];
						textureResourceHeight = (int)textureResourceNativeData[5];

                        // slice
                        if ((mask & 1) != 0)
                        {
                            ax = commands[i | 2];
                            ay = commands[i | 3];
                            cropWidth = commands[i | 4];
                            cropHeight = commands[i | 5];
							
							croppedLeft = textureLeft + textureWidth * ax / width;
							croppedRight = textureLeft + textureWidth * (ax + cropWidth) / width;
							croppedTop = textureTop + textureHeight * ay / height;
							croppedBottom = textureTop + textureHeight * (ay + cropHeight) / height;

                            textureLeft = croppedLeft;
                            textureRight = croppedRight;
                            textureTop = croppedTop;
                            textureBottom = croppedBottom;
                            width = cropWidth;
                            height = cropHeight;
                        }

                        // stretch
                        if ((mask & 2) != 0)
                        {
                            width = commands[i | 6];
                            height = commands[i | 7];
                        }

                        if (state == null ||
                            state.mode != 1 ||
                            state.a != alpha ||
                            state.textureId != textureId)
                        {
                            if (state == null)
                            {
                                state = new GlRenderState(0, 0, 0, 0, 0, 0);
                            }
                            else
                            {
                                GL.End();
                            }
                            state.mode = 1;
                            state.textureId = textureId;
                            state.a = alpha;
                            GL.Enable(EnableCap.Texture2D);
                            GL.Color4((byte)255, (byte)255, (byte)255, (byte)alpha);
                            GL.BindTexture(TextureTarget.Texture2D, textureId);
                            GL.Begin(BeginMode.Quads);
                        }

                        if (rotated)
                        {
                            angle = commands[i | 10] / 1048576.0;
                            r_cos = Math.Cos(angle);
                            r_sin = Math.Sin(angle);
                            tx = width * 0.5;
                            ty = height * 0.5;
                            ax = (r_cos * -tx - r_sin * -ty) + startX;
                            ay = (r_sin * -tx + r_cos * -ty) + startY;
                            bx = (r_cos * tx - r_sin * -ty) + startX;
                            by = (r_sin * tx + r_cos * -ty) + startY;
                            cx = (r_cos * -tx - r_sin * ty) + startX;
                            cy = (r_sin * -tx + r_cos * ty) + startY;
                            dx = (r_cos * tx - r_sin * ty) + startX;
                            dy = (r_sin * tx + r_cos * ty) + startY;
                            GL.TexCoord2(textureLeft, textureTop);
                            GL.Vertex2((int)(ax * RW / VW), (int)(ay * RH / VH));
                            GL.TexCoord2(textureRight, textureTop);
                            GL.Vertex2((int)(bx * RW / VW), (int)(by * RH / VH));
                            GL.TexCoord2(textureRight, textureBottom);
                            GL.Vertex2((int)(dx * RW / VW), (int)(dy * RH / VH));
                            GL.TexCoord2(textureLeft, textureBottom);
                            GL.Vertex2((int)(cx * RW / VW), (int)(cy * RH / VH));
                        }
                        else
                        {
                            endX = startX + width;
                            endY = startY + height;
                            GL.TexCoord2(textureLeft, textureTop);
                            GL.Vertex2(startX * RW / VW, startY * RH / VH);
                            GL.TexCoord2(textureRight, textureTop);
                            GL.Vertex2(endX * RW / VW, startY * RH / VH);
                            GL.TexCoord2(textureRight, textureBottom);
                            GL.Vertex2(endX * RW / VW, endY * RH / VH);
                            GL.TexCoord2(textureLeft, textureBottom);
                            GL.Vertex2(startX * RW / VW, endY * RH / VH);
                        }
                        break;

                    default:
                        return;
                }
                i += 16;
            }
            if (state != null)
            {
                GL.End();
            }
        }
    }
}
