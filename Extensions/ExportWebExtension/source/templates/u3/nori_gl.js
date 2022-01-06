const NoriGL = (() => {

    let cmdLog = s => {
        console.log(s);
    };

    let glDrawModes = {

    };

    let applyCommand = (gl, dataTranslate, cmd, buffer, startIndex, errCb) => {

        switch (cmd) {
            case 'INIT':
                // nothing to do here
                cmdLog("INIT: Initialize (nothing to do)");
                break;

            case 'AB-S':
                {
                    cmdLog("AB-S: Array Buffer - STATIC_DRAW");

                    let bufferId = buffer[startIndex++];
                    let bufferCount = buffer[startIndex++];
                    let items = [];
                    for (let i = 0; i < bufferCount; i++) {
                        items.push(buffer[startIndex + i]);
                    }
                    let arrBuf = gl.createBuffer();
                    dataTranslate['AB' + bufferId] = arrBuf;
                    gl.bindBuffer(gl.ARRAY_BUFFER, arrBuf);
                    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(items), gl.STATIC_DRAW);
                }
                break;

            case 'AS':
                {
                    cmdLog("AS: Attach Shader");
                    let programId = buffer[startIndex];
                    let program = dataTranslate['PROG' + programId];
                    let shaderId = buffer[startIndex + 1];
                    let shader = dataTranslate['SH' + shaderId];
                    gl.attachShader(program, shader);
                }
                break;

            case 'BND-UNI-MAT':
                {
                    cmdLog("BND-UNI-MAT: Bind Uniform Matrix");
                    let locId = buffer[startIndex];
                    let loc = dataTranslate['LOC' + locId];
                    let mat = [];
                    for (let i = 1; i <= 16; i++) {
                        mat.push(buffer[startIndex + i]);
                    }
                    gl.uniformMatrix4fv(loc, false, mat);
                }
                break;

            case 'BND-VER-BUF':
                {
                    cmdLog("BND-VER-BUF: Bind Vertex Buffer");
                    let locId = buffer[startIndex];
                    let bufferId = buffer[startIndex + 1];
                    let size = buffer[startIndex + 2];

                    let loc = dataTranslate['LOC' + locId];
                    let vBuffer = dataTranslate['AB' + bufferId];

                    gl.enableVertexAttribArray(loc);
                    gl.bindBuffer(gl.ARRAY_BUFFER, vBuffer);
                    gl.vertexAttribPointer(loc, size, gl.FLOAT, false, 0, 0);
                }
                break;

            case 'CP':
                {
                    cmdLog("CP: Create Program");
                    const program = gl.createProgram();
                    const id = buffer[startIndex];
                    dataTranslate['PROG' + id] = program;
                }
                break;

            case 'DRAW-ARR':
                {
                    cmdLog("DRAW-ARR: Draw Arrays");
                    let modeId = buffer[startIndex];
                    let start = buffer[startIndex + 1];
                    let count = buffer[startIndex + 2];
                    let mode;
                    switch (modeId) {
                        case 1: mode = gl.POINTS; break;
                        case 2: mode = gl.LINE_STRIP; break;
                        case 3: mode = gl.LINE_LOOP; break;
                        case 4: mode = gl.LINES; break;
                        case 5: mode = gl.TRIANGLE_STRIP; break;
                        case 6: mode = gl.TRIANGLE_FAN; break;
                        case 7: mode = gl.TRIANGLES; break;
                    }
                    gl.drawArrays(mode, start, count);
                }
                break;

            case 'EN-DEP':
                {
                    cmdLog("EN-DEP: Enable Depth Test");
                    gl.enable(gl.DEPTH_TEST);
                }
                break;

            case 'GLSL':
                {
                    cmdLog("GLSL: shader source");
                    const shader = gl.createShader(buffer[startIndex + 2] === 1 ? gl.VERTEX_SHADER : gl.FRAGMENT_SHADER);
                    gl.shaderSource(shader, buffer[startIndex + 1]);
                    gl.compileShader(shader);
                    dataTranslate['SH' + buffer[startIndex]] = shader;
                }
                break;

            case 'LP':
                {
                    cmdLog("LP: link program");
                    let programId = buffer[startIndex];
                    let program = dataTranslate['PROG' + programId];
                    gl.linkProgram(program);
                }
                break;

            case 'PU':
                {
                    cmdLog("PU: Program Use");
                    let programId = buffer[startIndex];
                    let program = dataTranslate['PROG' + programId];
                    gl.useProgram(program);
                }
                break;

            case 'QA':
                {
                    cmdLog("QA: Query attribute/uniform location");
                    let isUniform = buffer[startIndex] === 1;
                    let name = buffer[startIndex + 1];
                    let refId = buffer[startIndex + 2];
                    let programId = buffer[startIndex + 3];
                    let program = dataTranslate['PROG' + programId];

                    const loc = isUniform ? gl.getUniformLocation(program, name) : gl.getAttribLocation(program, name);
                    dataTranslate['LOC' + refId] = loc;
                }
                break;

            default:
                cmdLog("OI! **** " + cmd + " **** isn't implemetned yet!");
                break;
        }
    };

    return { applyCommand };
})();
