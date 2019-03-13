# Basic Animations

## Image animations

```csharp
class AnimatedImage {
    field images;
    field framesPerImage = 1;

    constructor(images, framesPerImage) {
        this.images = images.clone();
        this.framesPerImage = framesPerImage;
    }

    function getImage(counter) {
        return this.images[(counter / framesPerImage) % this.images.length];
    }
}
```

## Interpolation

```csharp
class TimedStoryboard {
    field startPoint;
    field endPoint;
    field duration;
    field startTime = null;
    field currentPoint;

    constructor(startX, startY, endX, endY, duration) {
        this.startPoint = [startX, startY];
        this.endPoint = [endX, endY];
        this.currentPoint = this.startPoint.clone();
        this.duration = duration;
    }

    function update() {
        now = Core.currentTime();
        if (this.startTime == null) {
            this.startTime = now;
        }

        Elapsed = now - this.startTime;
        progress = Math.min(1.0, elapsed / this.duration);
        antiProgress = 1.0 - progress;

        this.currentPoint[0] = this.startPoint[0] * antiProgress + this.endPoint[0] * progres;
        this.currentPoint[1] = this.startPoint[1] * antiProgress + this.endPoint[1] * progres;
    }
}
```

## Paths

```csharp
class FollowPathAnimation {

    // The list of points that the object will walk through.
    // You can add to this by calling .addPoint(x, y)
    field points = [];

    // The current location of the walking object.
    field currentX;
    field currentY;

    // The distance the object can walk per frame.
    field perFrameVelocity;

    constructor(currentX, currentY, velocity) {
        this.currentX = currentX;
        this.currentY = currentY;
        this.perFrameVelocity = velocity;
    }

    // Adds another point to the animation path.
    function addPoint(x, y) {
        this.points.add([x, y]);
    }

    // This would be called during each frame.
    function update() {
        this.updateImpl(this.perFrameVelocity);
    }

    function updateImpl(distanceToMove) {
        if (points.length == 0) return;
        next = this.points[0];
        dx = next[0] - this.currentX;
        dy = next[1] - this.currentY;
        distanceFromNext = (dx ** 2 + dy ** 2) ** .5;

        // Figure out if the distance to the next point is more
        // or less than the distance you will move this frame.
        if (distanceFromNext < distanceToMove) {
            this.currentX = next[0];
            this.currentY = next[1];
            this.points.remove(0);
            // If you didn’t move the entire distance, let
            // it carry over into the next point in the path.
            this.updateImpl(distanceToMove - distanceFromNext);
        } else {
            // If the point is further away than the distance
            // you can walk during this frame, then move the
            // current coordinates closer, but don't change the
            // point list.
            unitVectorX = dx / distance;
            unitVectorY = dy / distance;
            movementX = distanceToMove * unitVectorX;
            movementY = distanceToMove * unitVectorY;
            this.currentX += movementX;
            this.currentY += movementY;
        }
    }
}
```
