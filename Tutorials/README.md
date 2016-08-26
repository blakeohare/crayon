# Tutorial Tutorial

This is where tutorials on the site are stored and read from. If you'd like to add a tutorial
to the site, it's as simple as making a pull request.

Create a new directory in this directory and place a file in it called `index.html`. 
The name of this directory will be the URL to the tutorial. 
For example `crayon/Tutorials/{your-directory}/index.xml` 
becomes `http://crayonlang.org/tutorials/{your-directory}`

The root of each document is a <content> element with a variety of attributes.

    <content
      title="Name of the tutorial"
      author="your user ID"
      versions="current version of Crayon">
       
      tutorial content goes here.
      
    </content>

- title - The title as it will appear on the site.
- author - Your github username. 
- versions - This is a comma-delimited list of versions that this tutorial is applicable for. As new releases occur, this ought to be updated along with the content.

## `<header>`
You can create a header with the `<header>` and `</header>` tags. However, it is recommended you use the shorthand instead, 
which is placing a ! at the front of a line that has no tags in it.

    Text content
    !Heading Line
    More Text content

This is equivalent of doing this:

    Text content
    <header>Heading Line</header>
    More Text content

## `<code>`
What would a code tutorial be without displaying formatted code. Use the `<code>` tag for this.

    <code syntax="crayon">
        for (i = 0; i < list.length; ++i) {
          foo(list[i]);
        }
    </code>

The `<code>` tag supports an optional syntax attribute which will use a syntax highlighter. Supported syntaxes are:
* crayon
* xml

Other attributes include
* linenumbers - set to `true` to include line numbers
* startingline - set to a number to start the line numbers at something other than 1
* special - comma-delimited list of special keywords to color differently than general variables. For example, class names.

## `<b>`
Makes the text in it bold

## `<warning>`
Adds a scray pink tip box

## `<note>`
Adds a yellow tip box

## `<image>`
Insert an image. That path to this image should be relative to your index.xml file.

## `<link>`
Inserts a link. Supports multiple kinds of links denoted by including one of the following attributes.
* url - Just a direct URL. The value should be the URL including http[s]://. Only use this if none of the other options work.
* doc - a path to a documentation item This should be the namespace-qualified name. e.g. `<link doc="core.print">`. Do not include the version number. It will link to the same version as the tutorial version.

## `<table>`
Inserts a table. 

Elements directly inside a `<table>` element must be `<row>` elements. 

Elements directly inside a `<row>` element must be `<cell>` elements.

Most non-block tags can be included in `<cell>`

`<row>` supports the following attributes:
* header - set to true to make this a header. Multiple headers can exist.

## `<list>`
Inserts a bulleted or numbered list.

All elements inside a `<list>` element must be `<item>` elements.

`<list>` supports the following attributes:
* type - one of the following values:
** bullets (default)
** numbers
