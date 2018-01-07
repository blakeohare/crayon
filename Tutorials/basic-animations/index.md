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
	field points = [];
	field currentX;
	field currentY;
	field perFrameVelocity;
	
	constructor(currentX, currentY, velocity) {
		this.currentX = currentX;
		this.currentY = currentY;
		this.perFrameVelocity = velocity;
	}

	function addPoint(x, y) {
		this.points.add([x, y]);
	}

	function update() {
		this.updateImpl(this.perFrameVelocity);
	}

	function updateImpl(distanceRemaining) {
		if (points.length == 0) return;
		next =	this.points[0];
		dx = next[0] - this.currentX;
		dy = next[1] - this.currentY;
		distance = (dx ** 2 + dy ** 2) ** .5;
		if (distance < distanceRemaining) {
			this.currentX = next[0];
			this.currentY = next[1];
			this.points.remove(0);
			// If you didn’t move the entire distance, let
			// it carry over into the next point in the path.
			this.updateImpl(distanceRemaining - distance);
		} else {
			unitVectorX = dx / distance;
			unitVectorY = dy / distance;
			movementX = distanceRemaining * unitVectorX;
			movementY = distanceRemaining * unitVectorY;
			this.currentX += movementX;
			this.currentY += movementY;
		}
	}
}
```
