<?php
  
  class Rf {
    public $r;
    function __construct($value) {
      $this->r = $value;
    }
  }
  
  function pth_noop() { }
  
	function pth_new_array($size) {
    $output = array();
    while ($size-- > 0) array_push($output, null);
    return new Rf($output);
  }
  
  function pth_dictionary_get_keys($dictionary) {
    $output = array();
    foreach ($dictionary->r as $key => $ignored) {
      array_push($output, $key);
    }
    return new Rf($output);
  }
  
  $pth_program_data = null;
  function pth_setProgramData($pd) {
    $GLOBALS['pth_program_data'] = $pd;
  }
  
  function pth_getProgramData() {
    return $GLOBALS['pth_program_data'];
  }
  
  function pth_list_reverse($list) {
    $length = count($list->r);
    for ($i = intval($length / 2) - 1; $i >= 0; --$i) {
      $j = $length - $i - 1;
      $t = $list->r[$i];
      $list->r[$i] = $list->r[$j];
      $list->r[$j] = $t;
    }
  }
  
  function pth_getHttpRequest($stringOut) {
    $stringOut->r[0] = $_SERVER['REQUEST_METHOD'];
    $stringOut->r[1] = $_SERVER['REQUEST_URI'];
    $stringOut->r[2] = $_SERVER['REMOTE_HOST'];
  }
?>